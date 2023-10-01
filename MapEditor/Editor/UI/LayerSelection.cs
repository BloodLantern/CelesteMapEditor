namespace Editor.UI
{
    public class LayerSelection : UIComponent
    {
        private const int LayerTypes = 2;

        public MapViewer.Layers[] SelectedLayers = new MapViewer.Layers[LayerTypes];
        public MapViewer.DebugLayers SelectedDebugLayers;
        public string[] LayerTypeNames = new string[LayerTypes]
        {
            "Rendering",
            "Selection"
        };

        public LayerSelection(Application app)
            : base(app, RenderingCall.StateEditor)
        {
        }

        public override void Render()
        {
        }
    }
}
