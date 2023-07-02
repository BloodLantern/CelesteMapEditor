using Editor.Celeste;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Windows.Forms;
using SixLabors.Fonts;
using System.Net.NetworkInformation;

namespace Editor
{
    public class MapViewer
    {
        public Session Session;
        public PictureBox PictureBox;
        public RectangleF CameraBounds;

        private readonly TextOptions debugTextOptions;
        private static readonly Color debugTextBackgroundColor = Color.ParseHex("#10101050");

        private Point? lastMousePosition;

        public MapViewer(Session session, PictureBox pictureBox)
        {
            Session = session;
            PictureBox = pictureBox;
            CameraBounds = new RectangleF(0, 0, pictureBox.Width, pictureBox.Height);

            debugTextOptions = new(Session.DebugTextFont);
        }

        public void Render()
        {
            if (Session.CurrentMap == null)
                return;

            Image<Rgba32> result = new((int) CameraBounds.Width, (int) CameraBounds.Height, Color.DarkSlateGray);

            foreach (LevelData levelData in Session.CurrentMap.Levels)
            {
                if (!CameraBounds.IntersectsWith(levelData.Bounds))
                    continue;

                foreach (EntityData entityData in levelData.Entities)
                {
                    if (entityData.Position.X > CameraBounds.Right
                        || entityData.Position.Y > CameraBounds.Bottom
                        || entityData.Position.X + entityData.Size.Width < CameraBounds.Left
                        || entityData.Position.Y + entityData.Size.Height < CameraBounds.Top)
                        continue;

                    Entity entity = new(entityData);
                    if (entity.Texture != null)
                        result.Mutate(i => i.DrawImage(entity.Texture.Image, 1f));
                }
            }

            result.Mutate(
                i => i.Fill(debugTextBackgroundColor, new RectangleF(0, 0, 500, 100))
                .DrawText(debugTextOptions, $"Camera bounds: {CameraBounds}", Color.White)
            );

            PictureBox.Image = result.ToBitmap();
        }

        public void UpdatePosition(System.Drawing.Point mousePosition)
        {
            Point mousePos = new(mousePosition.X, mousePosition.Y);

            PointF dragDelta = PointF.Empty;
            if (lastMousePosition != null)
                dragDelta = new PointF(mousePos.X - lastMousePosition.Value.X, mousePos.Y - lastMousePosition.Value.Y);

            CameraBounds.Offset(dragDelta);

            lastMousePosition = mousePos;
        }
    }
}
