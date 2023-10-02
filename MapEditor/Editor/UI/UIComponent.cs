namespace Editor.UI
{
    public abstract class UIComponent
    {
        public readonly RenderingCall Call;

        public UIComponent(RenderingCall calls) => Call = calls;

        public abstract void Render();
    }
}
