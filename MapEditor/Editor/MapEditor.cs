using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Editor.ImGuiNet;
using Editor.Celeste;
using Editor.Extensions;
using System.IO;
using Editor.Logging;
using MonoGame.Extended.Input;
using Editor.Utils;
using MonoGame.Extended;
using System.Diagnostics;
using System.Reflection;

namespace Editor
{
    public class MapEditor : Game
    {
        private const string BaseTitle = "CelesteMapEditor";
        private const int BaseWindowWidth = 1280;
        private const int BaseWindowHeight = 720;

        public static MapEditor Instance { get; private set; }

        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;

        public ImGuiRenderer ImGuiRenderer;

        private FrameCounter frameCounter = new();

        public Session Session;
        public MapViewer MapViewer;
        public MenuBar MenuBar;

        public Point WindowSize => new(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        public MapEditor()
        {
            if (Instance != null)
                throw new InvalidOperationException($"There should be only one {nameof(MapEditor)} instance.");
            Instance = this;

            Logger.Log($"Starting {nameof(MapEditor)} instance...");

            /*Logger.Log("Testing logger...");
            Logger.Test();*/

            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.ClientSizeChanged += OnWindowResize;
            Window.FileDrop += OnFileDrop;
        }

        protected override void Initialize()
        {
            Logger.Log($"Initializing {nameof(MapEditor)}...");
            ImGuiRenderer = new ImGuiRenderer(this);

            Window.AllowUserResizing = true;
            Graphics.PreferredBackBufferWidth = BaseWindowWidth;
            Graphics.PreferredBackBufferHeight = BaseWindowHeight;
            Graphics.ApplyChanges();

            Session = new(this);
            // If an error occured while creating the Session instance, exit the program
            if (Session.Current == null)
                Exit();
            MapViewer = new(this);
            MenuBar = new(this);

            Graphics.SynchronizeWithVerticalRetrace = Session.Config.Vsync;
            TargetElapsedTime = Session.Config.MapViewerRefreshRate;
            IsFixedTimeStep = Session.Config.UseMapViewerRefreshRate;
            Graphics.ApplyChanges();

            Window.Title = BaseTitle;

            Logger.Log($"Working directory: {Directory.GetCurrentDirectory()}");
            if (Session.Config.LastEditedFiles.Count > 0)
                MapViewer.CurrentMap = LoadMap(Path.GetFullPath(Session.Config.LastEditedFile));

            base.Initialize();
            Logger.Log($"{nameof(MapEditor)} initialization complete.");
        }

        protected override void LoadContent()
        {
            Logger.Log($"Loading content...");
            SpriteBatch = new(GraphicsDevice);

            Session.LoadContent(Content);
            Logger.Log($"Content loaded.");
        }

        protected override void Update(GameTime time)
        {
            KeyboardStateExtended keyboardState = KeyboardExtended.GetState();

            MapViewer.Update(MouseExtended.GetState(), keyboardState);

            // Toggle debug mode with F3
            if (keyboardState.WasKeyJustUp(Microsoft.Xna.Framework.Input.Keys.F3))
                Session.Config.DebugMode = !Session.Config.DebugMode;

            Coroutine.UpdateAll(time);
            Logger.UpdateLogsAsync();

            frameCounter.Update(time.GetElapsedSeconds());

            base.Update(time);
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            MapViewer.Render(SpriteBatch);
            SpriteBatch.DrawString(Session.DebugFont, frameCounter.ToString(), new Vector2(10, 30), Color.White);
            SpriteBatch.End();

            base.Draw(time);

            ImGuiRenderer.BeforeLayout(time);
            if (Session.Config.DebugMode)
                MapViewer.RenderDebug();
            MenuBar.Render(Session);
            ImGuiRenderer.AfterLayout();
        }

        protected override void EndRun()
        {
            base.EndRun();

            Session.Exit();
            Logger.Log($"Stopping {nameof(MapEditor)} instance...");
            Logger.EndLogging(Session);
        }

        public Map LoadMap(string filepath)
        {
            MapData map = new(filepath);

            if (!map.Load())
                return null;

            Map result = new(map);

            MapViewer.CurrentMap = result;

            Session.Config.AddEditedFile(filepath);

            Window.Title = BaseTitle + " - " + Path.GetFileName(filepath);
            if (map.Area != AreaKey.None)
                Window.Title += " - " + map.Area;

            /*ListViewItem room;
            foreach (LevelData level in map.Levels)
            {
                room = new ListViewItem
                {
                    Text = level.Name.StartsWith("lvl_") ? level.Name[4..] : level.Name,
                    Tag = level
                };
                RoomList.Items.Add(room);
            }
            for (int i = 0; i < map.Fillers.Count; i++)
            {
                room = new ListViewItem
                {
                    Text = $"FILLER{i}",
                    Tag = map.Fillers[i]
                };
                RoomList.Items.Add(room);
            }*/

            return result;
        }

        private void OnWindowResize(object sender, EventArgs e) => MapViewer?.Camera.UpdateSize();

        private void OnFileDrop(object sender, FileDropEventArgs e)
        {
            if (e.Files.Length != 1)
                return;

            LoadMap(e.Files[0]);
        }

        public void Restart()
        {
            Logger.Log($"Restarting {nameof(MapEditor)}...");

            Process.Start(Environment.ProcessPath);

            Exit();
        }
    }
}