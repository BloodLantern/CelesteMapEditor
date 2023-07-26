using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;
using ImGuiNET;
using System;
using Editor.ImGuiNet;
using Editor.Celeste;
using System.IO;
using Editor.Logging;
using MonoGame.Extended.Input;
using Editor.Utils;

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

        public Session Session;
        public MapViewer MapViewer;
        
        public Point WindowSize => new(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);

        public MapEditor()
        {
            if (Instance != null)
                throw new InvalidOperationException($"There should be only one {nameof(MapEditor)} instance.");
            Instance = this;

            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ImGuiRenderer = new ImGuiRenderer(this);
            ImGuiRenderer.RebuildFontAtlas();

            Window.AllowUserResizing = true;
            Graphics.PreferredBackBufferWidth = BaseWindowWidth;
            Graphics.PreferredBackBufferHeight = BaseWindowHeight;
            Graphics.ApplyChanges();

            Session = new(this);
            // If an error occured while creating the Session instance, exit the program
            if (Session.Current == null)
                Exit();
            MapViewer = new(this, Window.ClientBounds.Size.ToVector2());

            Graphics.SynchronizeWithVerticalRetrace = Session.Config.Vsync;
            TargetElapsedTime = Session.Config.MapViewerRefreshRate;
            IsFixedTimeStep = Session.Config.UseMapViewerRefreshRate;
            Graphics.ApplyChanges();

            Window.Title = BaseTitle;

            Logger.Log($"Working directory: {Directory.GetCurrentDirectory()}");
            MapViewer.CurrentMap = LoadMap(Path.GetFullPath(Path.Combine("..", "..", "..", "..", "maps", "7-Summit.bin")));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new(GraphicsDevice);

            Session.LoadContent(Content);
        }

        protected override void Update(GameTime time)
        {
            MapViewer.Update(time, MouseExtended.GetState(), KeyboardExtended.GetState());
            Coroutine.UpdateAll(time);

            base.Update(time);
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            MapViewer.Render(SpriteBatch);
            SpriteBatch.End();

            base.Draw(time);

            /*if (!Session.Config.DebugMode)
                return;*/

            ImGuiRenderer.BeforeLayout(time);
            MapViewer.RenderDebug();
            ImGuiRenderer.AfterLayout();
        }

        protected override void EndRun()
        {
            base.EndRun();

            Session.Exit();
        }

        public Map LoadMap(string filepath)
        {
            MapData map = new(filepath);

            if (!map.Load())
                return null;

            Map result = new(map);

            MapViewer.CurrentMap = result;

            Window.Title = BaseTitle + " - " + Path.GetFileName(filepath);
            if (map.Area != AreaKey.None)
                Window.Title += " - " + map.Area;

            /*UpdateRecentFileList(filePath);

            ListViewItem room;
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
            }

            MapViewer.Render();*/

            return result;
        }
    }
}