namespace Editor.UI
{
    public abstract class UIComponent
    {
        protected readonly Application app;
        public readonly RenderingCall Call;

        public UIComponent(Application app, RenderingCall calls)
        {
            this.app = app;
            Call = calls;
        }

        public abstract void Render();
    }
}
