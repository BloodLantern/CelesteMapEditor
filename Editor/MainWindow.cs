using System.IO;
using System.Windows.Forms;

namespace Editor
{
    public partial class MainWindow : Form
    {
        const string BaseTitle = "CelesteMapEditor";

        public Session Session = new();

        public MainWindow()
        {
            InitializeComponent();
            AllowDrop = true;
        }

        private void LoadMap(in string filePath)
        {
            MapData map = Session.CurrentMap = new(filePath);

            if (!map.Load())
                return;

            Text = BaseTitle + " - " + (map.Area == AreaKey.None ? "Unknown map" : map.Area.ToString());

            ListViewItem room;
            foreach (LevelData level in map.Levels)
            {
                room = new ListViewItem
                {
                    Text = level.Name
                };
                RoomList.Items.Add(room);
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

            // Make sure there is only one path
            if (files.Length > 1)
                return;

            // Make sure it is a file and not a directory
            if (File.Exists(files[0]))
                e.Effect = DragDropEffects.Move;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            // We already made sure in OnDragEnter that there were only a single file and that it was not a directory
            LoadMap(((string[]) e.Data.GetData(DataFormats.FileDrop))[0]);
        }
    }
}
