using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Editor.Celeste;
using Editor.Extensions;
using Editor.Logging;
using Editor.Saved;
using Editor.UI;
using Editor.UI.Components;
using Editor.Utils;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.ImGuiNet;

namespace Editor;

public class Application : Game
{
    public enum State
    {
        Editor,
        Loading
    }

    public static Application Instance { get; private set; }

    public readonly Version Version = new(0, 1, 0);
    private const string BaseWindowTitle = "Celeste Map Editor";
    private const int BaseWindowWidth = 1280;
    private const int BaseWindowHeight = 720;
    public string WindowTitle => $"{BaseWindowTitle} v{Version.ToString(3)}";

    public readonly GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;
    public RenderTarget2D GlobalRenderTarget;

    /// <summary>
    /// Used to render the whole UI.
    /// </summary>
    public ImGuiRenderer ImGuiRenderer;

    public Session Session;
    public State CurrentState = State.Loading;

    public MapViewer MapViewer;

    public RenderTarget2D ImGuiRenderTarget;
    public UiManager UiManager;

    public Loading Loading;
    public RenderTarget2D LoadingRenderTarget;

    public Point WindowSize => GraphicsDevice.PresentationParameters.Bounds.Size;

    public int Fps { get; private set; }
    public long Memory { get; private set; }
    public const float StatisticsUpdateInterval = 1f;

    public GameTime GameTime;
    public TimeSpan TotalGameTime => GameTime.TotalGameTime;
    public float TotalGameTimeSeconds => (float) TotalGameTime.TotalSeconds;
    public float LastTotalGameTime;

    private readonly List<(string, Stopwatch)> debugStrings = [];

    public Application()
    {
        if (Instance != null)
            throw new InvalidOperationException($"There should be only one {nameof(Application)} instance.");
        Instance = this;

        Logger.Log($"Starting {nameof(Application)} instance...");

        Graphics = new(this);
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

        UiManager = new(this);
        UiManager.AddComponent(new DebugConsole(Session));

        Window.AllowUserResizing = true;
        ReloadGraphics(Session.Config.Graphics);

        GlobalRenderTarget = new(GraphicsDevice, BaseWindowWidth, BaseWindowHeight);
        LoadingRenderTarget = new(GraphicsDevice, BaseWindowWidth, BaseWindowHeight);
        ImGuiRenderTarget = new(GraphicsDevice, BaseWindowWidth, BaseWindowHeight);

        Window.Title = WindowTitle;

        Logger.Log($"Working directory: {Directory.GetCurrentDirectory()}");

        base.Initialize();
        Logger.Log($"{nameof(Application)} initialization complete. Took {stopwatch.ElapsedMilliseconds}ms");

        (Loading = new(
            this,
            loading =>
            {
                Stopwatch sw = Stopwatch.StartNew();

                Logger.Log($"Beginning startup loading...");

                loading.CurrentText = "Loading Celeste version and paths";
                if (!Session.TryLoad())
                    Exit();
                loading.Progress += 0.1f;

                const float CelesteModsPreLoadProgressFactor = 0.4f;
                if (Session.EverestVersion != null)
                {
                    loading.CurrentText = "Preloading Celeste mods";
                    CelesteMod.PreLoadAll(Session, loading, CelesteModsPreLoadProgressFactor);
                }
                else
                {
                    loading.Progress += CelesteModsPreLoadProgressFactor;
                }

                loading.CurrentText = "Loading vanilla atlases";
                Atlas.LoadVanillaAtlases(Session.CelesteGraphicsDirectory, loading, 0.3f);
                loading.CurrentText = "Loading vanilla autotilers";
                Autotiler.LoadAutotilers(Session.CelesteGraphicsDirectory);
                loading.Progress += 0.05f;

                loading.CurrentText = "Loading map viewer";
                MapViewer = new(this);
                loading.Progress += 0.04f;

                loading.CurrentText = "Loading UI";
                UiManager.AddRange(
                    [
                        new ConfigurationEditor(this),
                        new ModDependencies(Session),
                        new LayerSelection(this),
                        new LevelList(MapViewer),
                        new ModExplorer(this),
                        new MenuBar(this),
                        new PerformanceSummary(this)
                    ]
                );
                loading.Progress += 0.05f;

                loading.CurrentText = "Initializing map viewer";
                MapViewer.Initialize();
                loading.Progress += 0.01f;

                loading.CurrentText = "Loading map";
                if (Session.Config.RecentEditedFiles.Count > 0)
                    MapViewer.CurrentMap = LoadMap(Path.GetFullPath(Session.Config.LastEditedFile));
                loading.Progress += 0.05f;

                Logger.Log($"Startup loading complete. Took {sw.ElapsedMilliseconds}ms");
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
        GameTime = time;
            
        // Clear ImGui focus if the mouse clicked (with middle or right click, left is done by default) outside of a window
        if (!ImGui.GetIO().WantCaptureMouse)
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) || ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
                ImGui.SetWindowFocus(null);
        }

        switch (CurrentState)
        {
            case State.Loading:
                if (Loading == null)
                    CurrentState = State.Editor;
                break;

            case State.Editor:
                KeyboardStateExtended keyboardState = KeyboardExtended.GetState();

                MapViewer.Update(time, MouseExtended.GetState(), keyboardState);

                // Toggle debug mode with F3
                if (keyboardState.WasKeyPressed(Keys.F3))
                    Session.Config.DebugMode = !Session.Config.DebugMode;

                // On loading state change
                if (Loading is { Ended: true, DrawAlpha: <= 0f })
                    Loading = null;

                break;
        }

        Coroutine.UpdateAll(time);
        if (Calc.OnInterval(TotalGameTimeSeconds, LastTotalGameTime, StatisticsUpdateInterval))
        {
            Fps = (int) MathF.Round(1f / time.GetElapsedSeconds());
            Memory = GC.GetTotalMemory(false);
        }
            
        UiManager.UpdateComponents(time);

        for (int i = 0; i < debugStrings.Count; i++)
        {
            if (debugStrings[i].Item2.GetElapsedSeconds() > 1f)
                debugStrings.RemoveAt(i--);
        }

        LastTotalGameTime = TotalGameTimeSeconds;
        Logger.UpdateLogsAsync();

        base.Update(time);
    }

    protected override void Draw(GameTime time)
    {
        GraphicsDevice.SetRenderTarget(ImGuiRenderTarget);
        GraphicsDevice.Clear(Color.Transparent);
        ImGuiRenderer.BeginLayout(time);

        UiManager.RenderComponents(RenderingCall.First);
        
        // Dockspace
        ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, Color.Transparent.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Color.Transparent.PackedValue);
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport().ID);
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();

        UiManager.RenderComponents(RenderingCall.AfterDockspace);

        if (Loading != null)
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1f - Loading.DrawAlpha);

        switch (CurrentState)
        {
            case State.Editor:
                UiManager.RenderComponents(RenderingCall.StateEditor);
                if (Session.Config.DebugMode)
                    MapViewer.RenderDebug(time);
                break;

            case State.Loading:
                UiManager.RenderComponents(RenderingCall.StateLoading);
                break;
        }

        if (Loading != null)
            ImGui.PopStyleVar();

        UiManager.RenderComponents(RenderingCall.Last);

        ImGuiRenderer.EndLayout();

        if (Loading != null)
        {
            // Loading RenderTarget
            bool darkStyle = Session.Config.Ui.Style == ImGuiStyles.Style.Dark;

            GraphicsDevice.SetRenderTarget(LoadingRenderTarget);
            GraphicsDevice.Clear(darkStyle ? new(0xFF202020) : Color.Gray);

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
            MapViewer.Draw(time, SpriteBatch);
            SpriteBatch.End();
        }

        // RenderTargets rendering
        GraphicsDevice.SetRenderTarget(null);

        SpriteBatch.Begin();

        if (CurrentState == State.Editor)
            SpriteBatch.Draw(GlobalRenderTarget, Vector2.Zero, Color.White);

        if (Loading != null)
            SpriteBatch.Draw(LoadingRenderTarget, Vector2.Zero, Color.White * Loading.DrawAlpha);

        SpriteBatch.Draw(ImGuiRenderTarget, Vector2.Zero, Color.White);

        for (int i = 0; i < debugStrings.Count; i++)
            SpriteBatch.DrawString(Session.ConsolasFont, debugStrings[i].Item1, new(0f, i * 20f), Color.Magenta);

        SpriteBatch.End();
            
        base.Draw(time);
    }

    protected override void EndRun()
    {
        UiManager.SaveComponents();

        // Save config
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

        switch (Path.GetExtension(e.Files[0]))
        {
            case ".bin":
                LoadMap(e.Files[0]);
                break;
            case ".zip":
                LoadModZip(e.Files[0]);
                break;
        }
    }

    public void Restart()
    {
        Logger.Log($"Restarting {nameof(Application)}...");

        Process.Start(Environment.ProcessPath!);

        Exit();
    }

    public void DebugString(string str) => debugStrings.Insert(0, new(str, Stopwatch.StartNew()));

    public void ReloadGraphics(GraphicsConfig config)
    {
        Logger.Log($"Reloading graphics...");

        Graphics.PreferredBackBufferWidth = BaseWindowWidth;
        Graphics.PreferredBackBufferHeight = BaseWindowHeight;
        Graphics.SynchronizeWithVerticalRetrace = config.Vsync;

        if (config.RefreshRate != 0 && !config.Vsync)
        {
            TargetElapsedTime = new(0, 0, 0, 0, config.RefreshRate);
            IsFixedTimeStep = true;
        }
        else
        {
            TargetElapsedTime = new(0, 0, 0, 0, 16);
            IsFixedTimeStep = false;
        }

        Graphics.ApplyChanges();

        Logger.Log($"Graphics reloaded successfully.");
    }
}
