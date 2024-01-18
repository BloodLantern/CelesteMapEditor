using Editor.Saved;

namespace Editor.UI
{
    public abstract class UiComponent
    {
        /// <summary>
        /// When to render the component.
        /// </summary>
        public readonly RenderingCall Call;

        /// <summary>
        /// Creates a new UI component rendering at the specified <see cref="RenderingCall"/>.
        /// </summary>
        /// <param name="calls">The <see cref="RenderingCall"/> at which the component should render.</param>
        public UiComponent(RenderingCall calls) => Call = calls;

        /// <summary>
        /// Called when added to the UI manager. When implementing this method, you should call
        /// <code>base.Initialize(manager)</code>
        /// after your implementation.
        /// </summary>
        /// <param name="manager">The <see cref="UiManager"/> to which the component was added.</param>
        internal virtual void Initialize(UiManager manager) => Load(manager.Config);

        /// <summary>
        /// Called when rendering the component. This is where the majority of the component's
        /// functionality should be implemented because this is called when ImGui is ready to render.
        /// </summary>
        public abstract void Render();

        /// <summary>
        /// Called when removed from the UI manager. Used to save data to the configuration file.
        /// </summary>
        /// <param name="config">The <see cref="Config"/> instance used by the application.</param>
        public virtual void Save(Config config) { }

        /// <summary>
        /// Called after <see cref="Initialize(UiManager)"/>. Used to load data from the configuration file.
        /// </summary>
        /// <param name="config">The <see cref="Config"/> instance used by the application.</param>
        public virtual void Load(Config config) { }
    }
}
