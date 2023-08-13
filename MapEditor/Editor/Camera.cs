using Editor.Extensions;
using Editor.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections;
using System.ComponentModel;
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

        public Camera(Vector2 centerPosition, float zoom = DefaultZoom)
        {
            Vector2 size = ZoomToSize(zoom);
            Bounds = new(centerPosition - size / 2, size);
            TargetZoom = this.zoom = zoom;
            TargetPosition = Position;
        }

        public bool IsMoving() => Coroutine.IsRunningAndNotEmpty(moveRoutineGuid);

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

            TargetZoom = targetZoom;
            float oldZoom = Zoom;

            for (float timer = 0f; timer < duration; timer = stopwatch.GetElapsedSeconds())
            {
                Zoom = Calc.EaseLerp(oldZoom, targetZoom, timer, duration, Ease.QuadOut);

                yield return null;
            }

            Zoom = targetZoom;
        }

        public void UpdateSize() => Size = ZoomToSize(Zoom);

        public static Vector2 ZoomToSize(float zoom) => MapEditor.Instance.WindowSize.ToVector2() / zoom;

        public static float SizeToZoom(Vector2 size) => MapEditor.Instance.WindowSize.ToVector2().Length() / size.Length();

        public Vector2 MapToWindow(Vector2 position) => (position - Position) * Zoom;

        public Point MapToWindow(Point position) => (position - Position.ToPoint()).Mul(Zoom);

        public Size2 MapToWindow(Size2 size) => size * Zoom;

        public RectangleF MapToWindow(RectangleF bounds) => new(MapToWindow((Vector2) bounds.Position), MapToWindow(bounds.Size));

        public Vector2 WindowToMap(Vector2 position) => position / Zoom + Position;

        public Point WindowToMap(Point position) => position.Div(Zoom) + Position.ToPoint();

        public float GetLineThickness() => Math.Max(Zoom, 1f);

        public float GetStringScale() => Math.Max(Zoom / 2, 0.5f);
    }
}
