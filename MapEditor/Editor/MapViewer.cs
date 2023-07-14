using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Windows.Forms;

namespace Editor
{
    public class MapViewer : Control
    {
        public Session Session;
        public Image<Rgba32> CurrentImage;
        public RectangleF CameraBounds;

        public Point? ClickStartPosition;
        public PointF? CameraStartPosition;
        public PointF dragDelta;
        public bool Dragging => ClickStartPosition != null && CameraStartPosition != null;

        private readonly TextOptions debugTextOptions;
        private static readonly Color debugTextBackgroundColor = Color.ParseHex("#10101050");
        private long lastDrawImageTime = 0;

        public MapViewer(Session session)
        {
            DoubleBuffered = true;

            Session = session;

            debugTextOptions = new(Session.DebugTextFont);
        }

        /// <summary>
        /// Updates the map viewer.
        /// </summary>
        /// <param name="mouseButtons">The current pressed mouse buttons.</param>
        /// <param name="mousePosition">The current mouse position.</param>
        /// <returns>Whether the map viewer should be rendered.</returns>
        public bool Update(MouseButtons mouseButtons, Point mousePosition)
        {
            if ((mouseButtons & MouseButtons.Right) == 0)
            {
                ClickStartPosition = null;
                CameraStartPosition = null;
                return false;
            }

            if (!Dragging)
            {
                ClickStartPosition = mousePosition;
                CameraStartPosition = new PointF(CameraBounds.X, CameraBounds.Y);
                return false;
            }

            if (ClickStartPosition.Value.X < 0
                || ClickStartPosition.Value.Y < 0
                || ClickStartPosition.Value.X >= CurrentImage.Width
                || ClickStartPosition.Value.Y >= CurrentImage.Height)
                return false;

            dragDelta = new(ClickStartPosition.Value.X - mousePosition.X, ClickStartPosition.Value.Y - mousePosition.Y);

            CameraBounds = new RectangleF(((PointF) CameraStartPosition).X + dragDelta.X, ((PointF) CameraStartPosition).Y + dragDelta.Y,
                Width, Height);

            return true;
        }
        
        public void Render()
        {
            if (Session.CurrentMap == null)
                return;

            Image<Rgba32> result = new(Width, Height, Color.DarkSlateGray);

            Stopwatch stopwatch = Stopwatch.StartNew();
            (int, int) rendered = Session.CurrentMap.Render(CameraBounds, result);
            long time = stopwatch.ElapsedMilliseconds;

            if (Session.Config.ShowDebugInfo)
                result.Mutate(
                    i => i.Fill(debugTextBackgroundColor, new RectangleF(0, 0, 500, 105))
                    .DrawText($"Camera bounds: {CameraBounds}", Session.DebugTextFont, Color.White, new PointF(0, 0))
                    .DrawText($"Rendered levels: {rendered.Item1}", Session.DebugTextFont, Color.White, new PointF(0, 15))
                    .DrawText($"Rendered entities: {rendered.Item2}", Session.DebugTextFont, Color.White, new PointF(0, 30))
                    .DrawText($"Time to render to image: {time}", Session.DebugTextFont, Color.White, new PointF(0, 45))
                    .DrawText($"Time to render to window: {lastDrawImageTime}", Session.DebugTextFont, Color.White, new PointF(0, 60))
                    .DrawText($"Approximate FPS: {1f / ((time + lastDrawImageTime) / 1000f)}", Session.DebugTextFont, Color.White, new PointF(0, 75))
                    .DrawText($"Target FPS: {Session.Config.MapViewerRefreshRate}", Session.DebugTextFont, Color.White, new PointF(0, 90))
                );

            CurrentImage = result;

            Invalidate();
            Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            Stopwatch stopwatch = Stopwatch.StartNew();
            e.Graphics.DrawImageUnscaled(CurrentImage.ToBitmap(), System.Drawing.Point.Empty);
            lastDrawImageTime = stopwatch.ElapsedMilliseconds;
        }
    }
}
