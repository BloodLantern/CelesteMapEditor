using ImGuiNET;
using System;

namespace Editor.UI
{
    public class LayerSelection : UIComponent
    {
        private enum LayerType : byte
        {
            Rendering,
            Selection,
            Count
        }

        private const float WindowWidth = 400f;
        private const int LayerTypes = (int) LayerType.Count;

        private readonly MapViewer.Layers[] selectedLayers = new MapViewer.Layers[LayerTypes];
        private MapViewer.DebugLayers selectedDebugLayers;

        private LayerType currentLayerType = LayerType.Rendering;
        public bool WindowOpen = true;
        private bool windowCollapsed = true;

        public LayerSelection(Application app)
            : base(RenderingCall.StateEditor)
        {
        }

        public override void Render()
        {
            MapViewer.Layers[] layers = Enum.GetValues<MapViewer.Layers>();
            MapViewer.DebugLayers[] debugLayers = Enum.GetValues<MapViewer.DebugLayers>();

            ImGui.SetNextWindowSize(new(currentLayerType == LayerType.Rendering ? WindowWidth : WindowWidth / 2f, Math.Max(layers.Length, debugLayers.Length) * ImGui.GetItemRectSize().Y * 1.75f));
            ImGui.SetNextWindowCollapsed(windowCollapsed);
            ImGui.Begin("Layers", ref WindowOpen, ImGuiWindowFlags.NoResize);

            if (ImGui.IsWindowCollapsed())
                windowCollapsed = false;

            LayerType[] types = Enum.GetValues<LayerType>();
            for (int i = 0; i < LayerTypes; i++)
            {
                if (ImGui.RadioButton(types[i].ToString(), currentLayerType == (LayerType) i))
                    currentLayerType = (LayerType) i;
            }

            ImGui.BeginChild("layerFlags", new(ImGui.GetContentRegionAvail().X * (currentLayerType == LayerType.Rendering ? 0.5f : 1f), - 1), true, ImGuiWindowFlags.NoFocusOnAppearing);
            int selectedLayersInt = (int) selectedLayers[(int) currentLayerType];
            foreach (MapViewer.Layers layer in layers)
                ImGui.CheckboxFlags(layer.ToString(), ref selectedLayersInt, (int) layer);
            selectedLayers[(int) currentLayerType] = (MapViewer.Layers) selectedLayersInt;
            ImGui.EndChild();

            if (currentLayerType == LayerType.Rendering)
            {
                ImGui.SameLine();
                ImGui.BeginChild("layerDebugFlags", new(-1), true, ImGuiWindowFlags.NoFocusOnAppearing);
                int selectedDebugLayersInt = (int) selectedDebugLayers;
                foreach (MapViewer.DebugLayers layer in debugLayers)
                    ImGui.CheckboxFlags(layer.ToString(), ref selectedDebugLayersInt, (int) layer);
                selectedDebugLayers = (MapViewer.DebugLayers) selectedDebugLayersInt;
                ImGui.EndChild();
            }

            ImGui.End();
        }
    }
}
