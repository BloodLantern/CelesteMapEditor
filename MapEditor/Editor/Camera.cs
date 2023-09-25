using Editor.Extensions;
using Editor.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System;
using System.Collections;
using System.Diagnostics;

namespace Editor
{
    public class Camera
    {
        public const float DefaultZoom = 1f;
        public const float DefaultZoomDuration = 0.25f;
        public const float DefaultMoveDuration = 0.25f;

        public RectangleF Bounds;
        public Vector2 Position { get => Bounds.Position; set => Bounds.Position = value; }
        public Vector2 Size { get => Bounds.Size; set => Bounds.Size = value; }

        private Guid moveRoutineGuid = Guid.Empty;
        public Vector2 TargetPosition { get; private set; }

        private Guid zoomRoutineGuid = Guid.Empty;
        private float zoom;
        public float TargetZoom { get; private set; }
        public float Zoom
        {
            get => zoom;
            set
            {
                // Zoom in the center
                Vector2 oldSize = Size;
                Size *= zoom / value;
                Position -= (Size - oldSize) / 2;
                zoom = value;
            }
        }

        private Vector2? cameraStartPosition;
        private Point? cameraMoveClickStartPosition;
        private bool Dragging => cameraStartPosition.HasValue && cameraMoveClickStartPosition.HasValue;
        private Vector2 dragDelta;

        public Application app;
        public Config config;

        public Camera(Application app, Vector2 centerPosition, float zoom = DefaultZoom)
        {
            Vector2 size = ZoomToSize(zoom);
            Bounds = new(centerPosition - size / 2, size);
            TargetZoom = this.zoom = zoom;
            TargetPosition = Position;

            this.app = app;
            config = app.Session.Config;
        }

        public bool IsMoving() => Coroutine.IsRunningAndNotEmpty(moveRoutineGuid);

        public void MoveToDefault() => MoveTo(Vector2.Zero);

        public void MoveTo(Vector2 targetCenterPosition, float duration = DefaultMoveDuration)
            => Coroutine.Start(MoveRoutine(targetCenterPosition, duration), ref moveRoutineGuid);

        private IEnumerator MoveRoutine(Vector2 targetCenterPosition, float duration)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Vector2 targetPosition = targetCenterPosition - Size / 2;
            TargetPosition = targetPosition;
            Vector2 oldPosition = Position;

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                Bounds.X = Calc.EaseLerp(oldPosition.X, targetPosition.X, timer, duration, Ease.QuadOut);
                Bounds.Y = Calc.EaseLerp(oldPosition.Y, targetPosition.Y, timer, duration, Ease.QuadOut);

                yield return null;
            }

            Position = targetPosition;
        }

        public bool IsZooming() => Coroutine.IsRunningAndNotEmpty(zoomRoutineGuid);

        public void ZoomToDefault(float duration = DefaultZoomDuration) => ZoomTo(DefaultZoom, duration);

        public void ZoomTo(float targetZoom, float duration = DefaultZoomDuration)
            => Coroutine.Start(ZoomRoutine(targetZoom, duration), ref zoomRoutineGuid);

        public void ZoomTo(Vector2 targetWindowPosition, float targetZoom, float duration = DefaultZoomDuration)
            => Coroutine.Start(ZoomRoutine(targetWindowPosition, targetZoom, duration), ref zoomRoutineGuid);

        private IEnumerator ZoomRoutine(Vector2 targetWindowPosition, float targetZoom, float duration)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            TargetZoom = targetZoom;
            float oldZoom = Zoom;

            Vector2 oldMapPosition;
            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                oldMapPosition = WindowToMap(targetWindowPosition);

                Zoom = Calc.EaseLerp(oldZoom, targetZoom, timer, duration, Ease.QuadOut);
                Position += oldMapPosition - WindowToMap(targetWindowPosition);

                yield return null;
            }

            oldMapPosition = WindowToMap(targetWindowPosition);
            Zoom = targetZoom;
            Position += oldMapPosition - WindowToMap(targetWindowPosition);
        }

        private IEnumerator ZoomRoutine(float targetZoom, float duration)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // TODO Fix zooming a lot at the same time gives an impression of lag
            TargetZoom = targetZoom;
            float oldZoom = Zoom;

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                Zoom = Calc.EaseLerp(oldZoom, targetZoom, timer, duration, Ease.QuadOut);

                yield return null;
            }

            Zoom = targetZoom;
        }

        public void HandleInputs(MouseStateExtended mouse, KeyboardStateExtended keyboard)
        {
            if (mouse.IsButtonUp(config.CameraMoveButton))
            {
                cameraStartPosition = null;
                cameraMoveClickStartPosition = null;
            }
            else if (!Dragging)
            {
                cameraStartPosition = Position;
                cameraMoveClickStartPosition = mouse.Position;
            }

            // If the click started inside the window
            if (cameraMoveClickStartPosition.HasValue && new RectangleF(Point.Zero, app.WindowSize).Contains(cameraMoveClickStartPosition.Value))
            {
                if (Dragging)
                    dragDelta = (mouse.Position - cameraMoveClickStartPosition.Value).ToVector2() / Zoom;

                if (cameraStartPosition.HasValue)
                    Position = cameraStartPosition.Value - dragDelta;
            }

            if (mouse.DeltaScrollWheelValue != 0)
                ZoomTo(mouse.Position.ToVector2(), TargetZoom * MathF.Pow(config.ZoomFactor, -mouse.GetDeltaScrollWheel()));
        }

        public void OnResize()
        {
            Vector2 oldSize = Size;
            Size = ZoomToSize(Zoom);
            Position -= (Size - oldSize) / 2;
        }

        public static Vector2 ZoomToSize(float zoom) => Application.Instance.WindowSize.ToVector2() / zoom;

        public static float SizeToZoom(Vector2 size) => Application.Instance.WindowSize.ToVector2().Length() / size.Length();

        public Vector2 MapToWindow(Vector2 position) => (position - Position) * Zoom;

        public Point MapToWindow(Point position) => (position - Position.ToPoint()).Mul(Zoom);

        public Size2 MapToWindow(Size2 size) => size * Zoom;

        public RectangleF MapToWindow(RectangleF bounds) => new(MapToWindow((Vector2) bounds.Position), MapToWindow(bounds.Size));

        public Vector2 WindowToMap(Vector2 position) => position / Zoom + Position;

        public Point WindowToMap(Point position) => position.Div(Zoom) + Position.ToPoint();

        public Vector2 WindowToMap(Size2 size) => size / Zoom;

        public RectangleF WindowToMap(RectangleF bounds) => new(WindowToMap((Vector2) bounds.Position), WindowToMap(bounds.Size));

        public float GetLineThickness() => Math.Max(Zoom, 1f);

        public float GetStringScale() => Math.Max(Zoom / 2, 0.5f);
    }
}
