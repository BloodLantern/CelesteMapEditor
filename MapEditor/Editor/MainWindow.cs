using Editor.Celeste;
using Editor.Logging;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Windows.Forms;

namespace Editor
{
    public partial class MainWindow : Form
    {
        const string BaseTitle = "CelesteMapEditor";

        public Session Session;
        public MapViewer MapViewer;

        private readonly Timer mapViewerUpdater;

        public MainWindow()
        {
            Session = new();

            // If an error happened during Session initialization, exit
            if (Session.CurrentSession == null)
                ExitEditor();

            MapViewer = new(Session);
            InitializeComponent();
            MapViewer.CameraBounds = new RectangleF(-MapViewer.Width / 2, -MapViewer.Height / 2, MapViewer.Width, MapViewer.Height);

            InitializeRecentFileList();

            if (Session.Config.AutoLoadLastEditedMap)
                LoadMap(Session.Config.LastEditedFile);

            mapViewerUpdater = new();
            mapViewerUpdater.Tick += UpdateMapViewer;
            mapViewerUpdater.Interval = Calc.Round(1000f / Session.Config.MapViewerRefreshRate);
            mapViewerUpdater.Start();
        }

        private void LoadMap(string filePath)
        {
            UnloadMap();

            MapData map = new(filePath);

            if (!map.Load())
                return;

            Session.CurrentMap = new Map(map);

            UpdateRecentFileList(filePath);

            Text = BaseTitle + " - " + Path.GetFileName(filePath);
            if (map.Area != AreaKey.None)
                Text += " - " + map.Area;

            ListViewItem room;
            foreach (LevelData level in map.Levels)
            {
                room = new ListViewItem
                {
                    Text = level.Name.StartsWith("lvl_") ? level.Name[4..] : level.Name
                };
                RoomList.Items.Add(room);
            }
            for (int i = 0; i < map.Fillers.Count; i++)
            {
                room = new ListViewItem
                {
                    Text = $"FILLER{i}"
                };
                RoomList.Items.Add(room);
            }

            MapViewer.Render();
        }

        private void UnloadMap()
        {
            Session.CurrentMap = null;

            RoomList.Items.Clear();
        }

        private void UpdateMapViewer(object sender, EventArgs e)
        {
            if (!ContainsFocus)
                return;

            System.Drawing.Point mousePosition = MapViewer.PointToClient(MousePosition);
            if (MapViewer.Update(MouseButtons, new Point(mousePosition.X, mousePosition.Y)))
                MapViewer.Render();
        }

        private void InitializeRecentFileList()
        {
            if (Session.Config.LastEditedFiles.Count > 0)
                openRecentToolStripMenuItem.Enabled = true;

            foreach (string path in Session.Config.LastEditedFiles)
                openRecentToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem(path));
        }

        private void UpdateRecentFileList(string filePath)
        {
            if (Session.Config.AddEditedFile(filePath))
                openRecentToolStripMenuItem.DropDownItems.Insert(0, new ToolStripMenuItem(filePath));
            else
            {
                openRecentToolStripMenuItem.DropDownItems.Clear();

                foreach (string path in Session.Config.LastEditedFiles)
                    openRecentToolStripMenuItem.DropDownItems.Add(path);
            }

            if (Session.Config.LastEditedFiles.Count > 0)
                openRecentToolStripMenuItem.Enabled = true;
        }

        private void Shutdown()
        {
            mapViewerUpdater.Stop();
            Session.Exit();
            Logger.ClearLoggingFiles();
        }

        private void ExitEditor()
        {
            Shutdown();
            Application.Exit();
        }

        private void RestartEditor()
        {
            Shutdown();
            Application.Restart();
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

        // We already made sure in OnDragEnter that there were only a single file and that it was not a directory
        private void OnDragDrop(object sender, DragEventArgs e) => LoadMap(((string[]) e.Data.GetData(DataFormats.FileDrop))[0]);

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) => ExitEditor();

        private void RestartToolStripMenuItem_Click(object sender, EventArgs e) => RestartEditor();

        private void MetadataToolStripMenuItem_Click(object sender, EventArgs e) => new MetadataWindow().Show();

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Celeste Map Binary File|*.bin",
                Multiselect = false,
                Title = "Open Celeste Map"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                LoadMap(dialog.FileName);
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e) => ExitEditor();

        private void OnRecentFileClicked(object sender, ToolStripItemClickedEventArgs e) => LoadMap(e.ClickedItem.Text);
    }
}
