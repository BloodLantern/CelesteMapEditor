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

        public MainWindow()
        {
            InitializeComponent();

            Session = new();

            InitializeRecentFileList();

            if (Session.Config.AutoLoadLastEditedMap)
                LoadMap(Session.Config.LastEditedFile);

            Timer mapViewerUpdater = new();
            mapViewerUpdater.Tick += UpdateMapViewer;
            mapViewerUpdater.Interval = 1000 / Session.Config.MapViewerRefreshRate;
            mapViewerUpdater.Start();
        }

        private void LoadMap(string filePath)
        {
            UnloadMap();

            MapData map = new(filePath);

            if (!map.Load())
                return;

            Session.CurrentMap = new Map(map);
            MapViewer = new(Session, mapViewerPictureBox);

            UpdateRecentFileList(filePath);

            Text = BaseTitle + " - " + Path.GetFileName(filePath);
            if (map.Area != AreaKey.None)
                Text += " - " + map.Area;

            ListViewItem room;
            foreach (LevelData level in map.Levels)
            {
                room = new ListViewItem
                {
                    Text = level.Name
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
            if (MapViewer == null)
                return;

            MapViewer.Update(MouseButtons, new Point(MousePosition.X, MousePosition.Y));
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

        private void ExitEditor()
        {
            Session.Exit();
            Logger.ClearLoggingFiles();
            Application.Exit();
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

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitEditor();
        }

        private void RestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
            ExitEditor();
        }

        private void MetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new MetadataWindow().Show();
        }

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

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            ExitEditor();
        }

        private void OnRecentFileClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            LoadMap(e.ClickedItem.Text);
        }
    }
}
