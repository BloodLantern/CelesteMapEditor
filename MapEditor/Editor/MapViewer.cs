using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Numerics;
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
        public Vector2 dragDelta;
        public bool Dragging => ClickStartPosition != null && CameraStartPosition != null;

        private static readonly Color debugTextBackgroundColor = Color.ParseHex("#10101050");
        private long lastDrawImageTime = 0;

        public MapViewer(Session session)
        {
            DoubleBuffered = true;

            Session = session;

            Location = new System.Drawing.Point(250, 24);
            Margin = new Padding(4, 3, 4, 3);
            Name = "MapViewer";
            Size = new System.Drawing.Size(1225, 762);
            TabIndex = 4;
            TabStop = false;
            CameraBounds = new RectangleF(-Width / 2, -Height / 2, Width, Height);
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
            }

            if (!Dragging)
            {
                ClickStartPosition = mousePosition;
                CameraStartPosition = new PointF(CameraBounds.X, CameraBounds.Y);
            }

            if (ClickStartPosition.HasValue
                && (ClickStartPosition.Value.X < 0
                || ClickStartPosition.Value.Y < 0
                || ClickStartPosition.Value.X >= Width
                || ClickStartPosition.Value.Y >= Height))
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
            Session.CurrentMap.Render(CameraBounds, result);
            long time = stopwatch.ElapsedMilliseconds;

            if (Session.Config.ShowDebugInfo)
            {
                int yPosition = 0;
                result.Mutate(
                    i => i.Fill(debugTextBackgroundColor, new RectangleF(0, 0, 500, 120))
                    .DrawText($"Camera bounds: {CameraBounds}", Session.DebugTextFont, Color.White, new PointF(0, yPosition))
                    .DrawText($"Time to render to image: {time}", Session.DebugTextFont, Color.White, new PointF(0, yPosition += 15))
                    .DrawText($"Time to render to window: {lastDrawImageTime}", Session.DebugTextFont, Color.White, new PointF(0, yPosition += 15))
                    .DrawText($"Approximate FPS: {1f / ((time + lastDrawImageTime) / 1000f)}", Session.DebugTextFont, Color.White, new PointF(0, yPosition += 15))
                    .DrawText($"Target FPS: {Session.Config.MapViewerRefreshRate}", Session.DebugTextFont, Color.White, new PointF(0, yPosition += 15))
                    .DrawText($"Mouse position: {MousePosition}", Session.DebugTextFont, Color.White, new PointF(0, yPosition += 15))
                );
            }

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
