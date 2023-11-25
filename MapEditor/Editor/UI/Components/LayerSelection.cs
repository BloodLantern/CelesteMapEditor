using Editor.Saved;
using ImGuiNET;
using System;

namespace Editor.UI.Components
{
    public class LayerSelection : UIComponent, ICloseable
    {
        public enum LayerType : byte
        {
            Rendering,
            Selection,
            Count
        }

        [Flags]
        public enum Layers : byte
        {
            None                = byte.MinValue,
            BackgroundTiles     = 1 << 0,
            BackgroundDecals    = 1 << 1,
            PlayerSpawns        = 1 << 2,
            Entities            = 1 << 3,
            ForegroundTiles     = 1 << 4,
            Fillers             = 1 << 5, // Rendering only, not selectable
            ForegroundDecals    = 1 << 6,
            Triggers            = 1 << 7,
            All                 = byte.MaxValue,
        }

        [Flags]
        public enum DebugLayers : byte
        {
            None                = byte.MinValue,
            LevelBounds         = 1 << 0,
            FillerBounds        = 1 << 1,
            EntityBounds        = 1 << 2,
            PlayerSpawns        = 1 << 3,
            Unused2             = 1 << 4,
            Unused3             = 1 << 5,
            Unused4             = 1 << 6,
            Unused5             = 1 << 7,
            All                 = byte.MaxValue,
        }

        private const float WindowWidth = 400f;
        public const int LayerTypes = (int) LayerType.Count;

        private Layers[] selectedLayers = new Layers[LayerTypes];
        private DebugLayers selectedDebugLayers;

        private LayerType currentLayerType = LayerType.Rendering;
        private bool windowOpen = false;
        private bool windowCollapsed = true;

        public bool WindowOpen { get => windowOpen; set => windowOpen = value; }
        public string KeyboardShortcut { get; set; }

        public LayerSelection(Application app)
            : base(RenderingCall.StateEditor)
        {
            windowOpen = app.Session.Config.ShowLayerSelectionWindow;
        }

        public override void Render()
        {
            if (!windowOpen)
                return;

            Layers[] layers = Enum.GetValues<Layers>();
            DebugLayers[] debugLayers = Enum.GetValues<DebugLayers>();

            ImGui.SetNextWindowSize(new(currentLayerType == LayerType.Rendering ? WindowWidth : WindowWidth / 2f, Math.Max(layers.Length, debugLayers.Length) * ImGui.GetItemRectSize().Y * 1.75f));
            ImGui.SetNextWindowCollapsed(windowCollapsed);
            ImGui.Begin("Layers", ref windowOpen, ImGuiWindowFlags.NoResize);

            if (ImGui.IsWindowCollapsed())
                windowCollapsed = false;

            LayerType[] types = Enum.GetValues<LayerType>();
            for (int i = 0; i < LayerTypes; i++)
            {
                if (ImGui.RadioButton(types[i].ToString(), currentLayerType == (LayerType) i))
                    currentLayerType = (LayerType) i;
            }

            ImGui.BeginChild("layerFlags", new(ImGui.GetContentRegionAvail().X * (currentLayerType == LayerType.Rendering ? 0.5f : 1f), -1), ImGuiChildFlags.Border);
            int selectedLayersInt = (int) selectedLayers[(int) currentLayerType];
            foreach (Layers layer in layers)
                ImGui.CheckboxFlags(layer.ToString(), ref selectedLayersInt, (int) layer);
            selectedLayers[(int) currentLayerType] = (Layers) selectedLayersInt;
            ImGui.EndChild();

            if (currentLayerType == LayerType.Rendering)
            {
                ImGui.SameLine();
                ImGui.BeginChild("layerDebugFlags", new(-1), ImGuiChildFlags.Border);
                int selectedDebugLayersInt = (int) selectedDebugLayers;
                foreach (DebugLayers layer in debugLayers)
                    ImGui.CheckboxFlags(layer.ToString(), ref selectedDebugLayersInt, (int) layer);
                selectedDebugLayers = (DebugLayers) selectedDebugLayersInt;
                ImGui.EndChild();
            }

            ImGui.End();
        }

        public override void Save(Config config)
        {
            base.Save(config);

            config.ShowLayerSelectionWindow = windowOpen;

            config.LayerSelectionLayers = selectedLayers;
            config.LayerSelectionDebugLayers = selectedDebugLayers;
        }

        public override void Load(Config config)
        {
            base.Load(config);

            windowOpen = config.ShowLayerSelectionWindow;

            selectedLayers = config.LayerSelectionLayers;
            selectedDebugLayers = config.LayerSelectionDebugLayers;
        }

        public Layers GetLayers(LayerType layerType) => selectedLayers[(int) layerType];

        public DebugLayers GetDebugLayers() => selectedDebugLayers;
    }
}
