﻿using Microsoft.Xna.Framework;
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
    public class Application : Game
    {
        public enum State
        {
            Editor,
            Loading
        }

        public static Application Instance { get; private set; }

        public readonly Version Version = new(0, 1, 0, 0);
        private const string BaseWindowTitle = "CelesteMapEditor";
        private const int BaseWindowWidth = 1280;
        private const int BaseWindowHeight = 720;
        public string WindowTitle => $"{BaseWindowTitle} v{Version.ToString(3)}";

        public GraphicsDeviceManager Graphics;
        public SpriteBatch SpriteBatch;
        public RenderTarget2D GlobalRenderTarget;

        /// <summary>
        /// Used to render the whole UI.
        /// </summary>
        public ImGuiRenderer ImGuiRenderer;

        public readonly FrameCounter FrameCounter = new();

        public Session Session;
        public State CurrentState = State.Loading;

        public MapViewer MapViewer;

        public RenderTarget2D ImGuiRenderTarget;
        public MenuBar MenuBar;
        public LeftPanel LeftPanel;
        public ModDependencies ModDependencies;
        public DebugConsole DebugConsole;

        public Loading Loading;
        public RenderTarget2D LoadingRenderTarget;

        public Point WindowSize => GraphicsDevice.PresentationParameters.Bounds.Size;

        public Application()
        {
            if (Instance != null)
                throw new InvalidOperationException($"There should be only one {nameof(Application)} instance.");
            Instance = this;

            Logger.Log($"Starting {nameof(Application)} instance...");

            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.ClientSizeChanged += OnWindowResize;
            Window.FileDrop += OnFileDrop;
        }

        protected override void Initialize()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Logger.Log($"Initializing {nameof(Application)}...");

            ImGuiRenderer = new(this);

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
            ImGuiRenderTarget = new(GraphicsDevice, BaseWindowWidth, BaseWindowHeight);

            Window.Title = WindowTitle;

            Logger.Log($"Working directory: {Directory.GetCurrentDirectory()}");

            base.Initialize();
            Logger.Log($"{nameof(Application)} initialization complete. Took {stopwatch.ElapsedMilliseconds}ms");

            (Loading = new(
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
                    MenuBar = new(this);
                    MapViewer = new(this);
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
            }).OnEnd += () =>
            {
                MapViewer.InitializeCamera();
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

                    if (Loading != null && Loading.Ended && Loading.DrawAlpha <= 0f)
                    {
                        LeftPanel.Visible = true;
                        LeftPanel.StartMoveInRoutine();

                        MenuBar.Visible = true;
                        MenuBar.StartMoveInRoutine();

                        Loading = null;
                    }

                    break;
            }

            Coroutine.UpdateAll(time);
            Logger.UpdateLogsAsync();

            FrameCounter.Update(time.GetElapsedSeconds());

            base.Update(time);
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.SetRenderTarget(ImGuiRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            ImGuiRenderer.BeforeLayout(time);

            if (Loading != null)
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1f - Loading.DrawAlpha);

            if (CurrentState == State.Editor)
            {
                MenuBar.Render(Session);

                if (Session.Config.DebugMode)
                    MapViewer.RenderDebug();

                LeftPanel.Render();
                ModDependencies.Render();
            }

            if (Loading != null)
                ImGui.PopStyleVar();

            if (Session.Config.ShowDebugConsole)
                DebugConsole.Render();

            ImGuiRenderer.AfterLayout();

            if (Loading != null)
            {
                // Loading RenderTarget
                bool darkStyle = Session.Config.UiStyle == ImGuiStyles.Style.Dark;

                GraphicsDevice.SetRenderTarget(LoadingRenderTarget);
                GraphicsDevice.Clear(darkStyle ? new Color(0xFF202020) : Color.Gray);

                SpriteBatch.Begin();
                Loading.Render(SpriteBatch, this, Session, time);
                SpriteBatch.End();
            }

            // Global RenderTarget
            GraphicsDevice.SetRenderTarget(GlobalRenderTarget);
            GraphicsDevice.Clear(Color.DarkSlateGray);

            if (CurrentState == State.Editor)
            {
                SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
                MapViewer.Render(time, SpriteBatch);
                SpriteBatch.End();
            }

            // RenderTargets rendering
            GraphicsDevice.SetRenderTarget(null);

            SpriteBatch.Begin();

            if (CurrentState == State.Editor)
                SpriteBatch.Draw(GlobalRenderTarget, Vector2.Zero, Color.White);

            if (Loading != null)
                SpriteBatch.Draw(LoadingRenderTarget, Vector2.Zero, Color.White * Loading.DrawAlpha);

            if (Session.Config.ShowAverageFps)
                FrameCounter.Render(SpriteBatch, Session, LeftPanel, MenuBar);

            SpriteBatch.Draw(ImGuiRenderTarget, Vector2.Zero, Color.White);

            SpriteBatch.End();
            
            base.Draw(time);
        }

        protected override void EndRun()
        {
            Session.Exit();
            Logger.Log($"Stopping {nameof(Application)} instance...");
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

        public void LoadModZip(string path)
        {
            throw new NotImplementedException();
        }

        private void OnWindowResize(object sender, EventArgs e)
        {
            GlobalRenderTarget = new(GraphicsDevice, WindowSize.X, WindowSize.Y);
            LoadingRenderTarget = new(GraphicsDevice, WindowSize.X, WindowSize.Y);
            ImGuiRenderTarget = new(GraphicsDevice, WindowSize.X, WindowSize.Y);

            MapViewer?.Camera.OnResize();
        }

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
            Logger.Log($"Restarting {nameof(Application)}...");

            Process.Start(Environment.ProcessPath);

            Exit();
        }
    }
}