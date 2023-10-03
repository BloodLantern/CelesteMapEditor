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

        private const int LayerTypes = (int) LayerType.Count;

        private readonly MapViewer.Layers[] selectedLayers = new MapViewer.Layers[LayerTypes];
        private MapViewer.DebugLayers selectedDebugLayers;

        private LayerType currentLayerType = LayerType.Rendering;

        public LayerSelection(Application app)
            : base(RenderingCall.StateEditor)
        {
        }

        public override void Render()
        {
            ImGui.Begin("Layers");

            LayerType[] types = Enum.GetValues<LayerType>();
            for (int i = 0; i < LayerTypes; i++)
            {
                if (ImGui.RadioButton(types[i].ToString(), currentLayerType.HasFlag((LayerType) i)))
                    currentLayerType |= (LayerType) i;
                else
                    currentLayerType &= ~(LayerType) i;
            }

            ImGui.BeginChild("layerFlags", new(ImGui.GetContentRegionAvail().X  * 0.5f, - 1), true, ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.AlwaysAutoResize);
            int selectedLayersInt = (int) selectedLayers[(int) currentLayerType];
            foreach (MapViewer.Layers layer in Enum.GetValues<MapViewer.Layers>())
                ImGui.CheckboxFlags(layer.ToString(), ref selectedLayersInt, (int) layer);
            selectedLayers[(int) currentLayerType] = (MapViewer.Layers) selectedLayersInt;
            ImGui.EndChild();

            if (currentLayerType == LayerType.Rendering)
            {
                ImGui.SameLine();
                ImGui.BeginChild("layerDebugFlags", new(-1), true, ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.AlwaysAutoResize);
                int selectedDebugLayersInt = (int) selectedDebugLayers;
                foreach (MapViewer.DebugLayers layer in Enum.GetValues<MapViewer.DebugLayers>())
                    ImGui.CheckboxFlags(layer.ToString(), ref selectedDebugLayersInt, (int) layer);
                selectedDebugLayers = (MapViewer.DebugLayers) selectedDebugLayersInt;
                ImGui.EndChild();
            }

            ImGui.End();
        }
    }
}
