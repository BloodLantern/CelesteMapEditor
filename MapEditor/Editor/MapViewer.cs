using Editor.Celeste;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Windows.Forms;
using SixLabors.Fonts;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Diagnostics;

namespace Editor
{
    public class MapViewer
    {
        public Session Session;
        public PictureBox PictureBox;
        public RectangleF CameraBounds;

        public Point? ClickStartPosition;
        public PointF? CameraStartPosition;
        public PointF dragDelta;
        public bool Dragging => ClickStartPosition != null && CameraStartPosition != null;

        private readonly TextOptions debugTextOptions;
        private static readonly Color debugTextBackgroundColor = Color.ParseHex("#10101050");


        public MapViewer(Session session, PictureBox pictureBox)
        {
            Session = session;
            PictureBox = pictureBox;
            CameraBounds = new RectangleF(-pictureBox.Width / 2, -pictureBox.Height / 2, pictureBox.Width, pictureBox.Height);

            debugTextOptions = new(Session.DebugTextFont);
        }

        public void Render()
        {
            if (Session.CurrentMap == null)
                return;

            Image<Rgba32> result = new(PictureBox.Width, PictureBox.Height, Color.DarkSlateGray);

            Stopwatch stopwatch = Stopwatch.StartNew();
            (int, int) rendered = Session.CurrentMap.Render(CameraBounds, result);
            long time = stopwatch.ElapsedMilliseconds;

            if (Session.Config.ShowDebugInfo)
                result.Mutate(
                    i => i.Fill(debugTextBackgroundColor, new RectangleF(0, 0, 500, 100))
                    .DrawText($"Camera bounds: {CameraBounds}", Session.DebugTextFont, Color.White, new PointF(0, 0))
                    .DrawText($"Rendered levels: {rendered.Item1}", Session.DebugTextFont, Color.White, new PointF(0, 15))
                    .DrawText($"Rendered entities: {rendered.Item2}", Session.DebugTextFont, Color.White, new PointF(0, 30))
                    .DrawText($"Time to render: {time}", Session.DebugTextFont, Color.White, new PointF(0, 45))
                    .DrawText($"Approximate FPS: {1f / (time / 1000f)}", Session.DebugTextFont, Color.White, new PointF(0, 60))
                    .DrawText($"Target FPS: {Session.Config.MapViewerRefreshRate}", Session.DebugTextFont, Color.White, new PointF(0, 75))
                );

            PictureBox.Image = result.ToBitmap();
            PictureBox.Refresh();
        }

        public void Update(MouseButtons mouseButtons, Point mousePosition)
        {
            if ((mouseButtons & MouseButtons.Right) == 0)
            {
                ClickStartPosition = null;
                CameraStartPosition = null;
                return;
            }

            if (!Dragging)
            {
                ClickStartPosition = mousePosition;
                CameraStartPosition = new PointF(CameraBounds.X, CameraBounds.Y);
                return;
            }

            dragDelta = new(ClickStartPosition.Value.X - mousePosition.X, ClickStartPosition.Value.Y - mousePosition.Y);

            CameraBounds = new RectangleF(((PointF) CameraStartPosition).X + dragDelta.X, ((PointF) CameraStartPosition).Y + dragDelta.Y,
                CameraBounds.Width, CameraBounds.Height);

            Render();
        }
    }
}
