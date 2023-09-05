using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Editor.Celeste;
using Editor.Extensions;
using System.IO;
using Editor.Logging;
using MonoGame.Extended.Input;
using Editor.Utils;
using MonoGame.Extended;
using System.Diagnostics;
using Editor.UI;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;
using ImGuiNET;

namespace Editor
{
    public class MapEditor : Game
    {
        public enum State
        {
            Editor,
            Loading
        }

        public static MapEditor Instance { get; private set; }

        public readonly Version Version = new(0, 1, 0, 0);
        private const string BaseWindowTitle = "CelesteMapEditor";
        private const int BaseWindowWidth = 1280;
        private const int BaseWindowHeight = 720;
        public string WindowTitle => $"{BaseWindowTitle} v{Version.ToString(3)}";

        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;
        public RenderTarget2D GlobalRenderTarget;

        public ImGuiRenderer ImGuiRenderer;

        public readonly FrameCounter FrameCounter = new();

        public Session Session;
        public State CurrentState = State.Loading;

        public MapViewer MapViewer;

        public MenuBar MenuBar;
        public LeftPanel LeftPanel;
        public ModDependencies ModDependencies;
        public DebugConsole DebugConsole;

        public Loading Loading;
        public RenderTarget2D LoadingRenderTarget;

        public Point WindowSize => new(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        public MapEditor()
        {
            if (Instance != null)
                throw new InvalidOperationException($"There should be only one {nameof(MapEditor)} instance.");
            Instance = this;

            Logger.Log($"Starting {nameof(MapEditor)} instance...");

            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.ClientSizeChanged += OnWindowResize;
            Window.FileDrop += OnFileDrop;
        }

        protected override void Initialize()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Logger.Log($"Initializing {nameof(MapEditor)}...");
            ImGuiRenderer = new ImGuiRenderer(this);

            Session = new(this);

            DebugConsole = new(Session);

            Window.AllowUserResizing = true;
            Graphics.PreferredBackBufferWidth = BaseWindowWidth;
            Graphics.PreferredBackBufferHeight = BaseWindowHeight;
            Graphics.SynchronizeWithVerticalRetrace = Session.Config.Vsync;
            TargetElapsedTime = Session.Config.MapViewerRefreshRate;
            IsFixedTimeStep = Session.Config.UseMapViewerRefreshRate;
            Graphics.ApplyChanges();

            GlobalRenderTarget = new(GraphicsDevice, BaseWindowWidth, BaseWindowHeight);
            LoadingRenderTarget = new(GraphicsDevice, BaseWindowWidth, BaseWindowHeight);

            Window.Title = WindowTitle;

            Logger.Log($"Working directory: {Directory.GetCurrentDirectory()}");

            base.Initialize();
            Logger.Log($"{nameof(MapEditor)} initialization complete. Took {stopwatch.ElapsedMilliseconds}ms");

            Loading = new(
                this,
                (ref float progress, ref string resource) =>
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    Logger.Log($"Beginning startup loading...");

                    resource = "Loading Celeste version and paths";
                    if (!Session.TryLoad())
                        Exit();
                    progress += 0.1f;

                    resource = "Preloading Celeste mods";
                    if (Session.EverestVersion != null)
                        CelesteMod.PreLoadAll(Session, ref progress, 0.4f);

                    resource = "Loading vanilla atlases";
                    Atlas.LoadVanillaAtlases(Session.CelesteGraphicsDirectory);
                    progress += 0.3f;
                    resource = "Loading vanilla autotilers";
                    Autotiler.LoadAutotilers(Session.CelesteGraphicsDirectory);
                    progress += 0.1f;

                    resource = "Loading UI";
                    MapViewer = new(this);
                    MenuBar = new(this);
                    LeftPanel = new(this);
                    ModDependencies = new(Session);

                    progress += 0.05f;

                    resource = "Loading map";
                    if (Session.Config.LastEditedFiles.Count > 0)
                        MapViewer.CurrentMap = LoadMap(Path.GetFullPath(Session.Config.LastEditedFile));
                    progress += 0.05f;

                    Logger.Log($"Startup loading complete. Took {stopwatch.ElapsedMilliseconds}ms");
                }
            )
            {
                ShowRemainingTime = true
            };
            Loading.End += () =>
            {
                MapViewer.InitializeCamera();
                Coroutine.Start(Loading.FadeOutRoutine(Loading.FadeOutDuration));
                CurrentState = State.Editor;
            };
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
            switch (CurrentState)
            {
                case State.Loading:
                    if (Loading == null)
                        CurrentState = State.Editor;
                    break;

                case State.Editor:
                    KeyboardStateExtended keyboardState = KeyboardExtended.GetState();

                    MapViewer.Update(MouseExtended.GetState(), keyboardState);

                    // Toggle debug mode with F3
                    if (keyboardState.WasKeyJustUp(Keys.F3))
                        Session.Config.DebugMode = !Session.Config.DebugMode;

                    break;
            }

            Coroutine.UpdateAll(time);
            Logger.UpdateLogsAsync();

            FrameCounter.Update(time.GetElapsedSeconds());

            base.Update(time);
        }

        protected override void Draw(GameTime time)
        {
            ImGuiRenderer.BeforeLayout(time);

            // Global RenderTarget
            GraphicsDevice.SetRenderTarget(GlobalRenderTarget);
            GraphicsDevice.Clear(Color.DarkSlateGray);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            MapViewer?.Render(SpriteBatch);
            SpriteBatch.End();

            if (Loading == null)
            {
                if (Session.Config.DebugMode)
                    MapViewer?.RenderDebug();

                MenuBar?.Render(Session);
                LeftPanel?.Render();
                ModDependencies?.Render();
            }
            else
            {
                // Loading RenderTarget
                bool darkStyle = Session.Config.UiStyle == ImGuiStyles.Style.Dark;

                GraphicsDevice.SetRenderTarget(LoadingRenderTarget);
                GraphicsDevice.Clear(darkStyle ? new Color(0xFF202020) : Color.Gray);

                SpriteBatch.Begin();
                Loading.Render(SpriteBatch, this, Session, time);
                SpriteBatch.End();
            }

            // RenderTargets rendering
            GraphicsDevice.SetRenderTarget(null);

            SpriteBatch.Begin();

            SpriteBatch.Draw(GlobalRenderTarget, Vector2.Zero, Color.White);

            if (Loading != null)
                SpriteBatch.Draw(LoadingRenderTarget, Vector2.Zero, Color.White * Loading.DrawAlpha);

            // Frame counter
            FrameCounter.Render(SpriteBatch, Session, Loading == null ? LeftPanel : null);

            SpriteBatch.End();


            if (Session.Config.ShowDebugConsole)
                DebugConsole.Render();
            
            base.Draw(time);

            ImGuiRenderer.AfterLayout();
        }

        protected override void EndRun()
        {
            base.EndRun();

            Session.Exit();
            Logger.Log($"Stopping {nameof(MapEditor)} instance...");
            Logger.EndLogging(Session.Config);
        }

        public Map LoadMap(string filepath)
        {
            MapData map = new(filepath);

            if (!map.Load())
                return null;

            Map result = new(map);

            MapViewer.CurrentMap = result;

            Session.Config.AddEditedFile(filepath);

            Window.Title = WindowTitle + " - " + Path.GetFileName(filepath);
            if (map.Area != AreaKey.None)
                Window.Title += " - " + map.Area;

            return result;
        }

        private void OnWindowResize(object sender, EventArgs e) => MapViewer?.Camera.UpdateSize();

        private void OnFileDrop(object sender, FileDropEventArgs e)
        {
            if (CurrentState != State.Editor)
                return;

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