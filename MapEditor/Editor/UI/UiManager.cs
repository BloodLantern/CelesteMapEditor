using Editor.Saved;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Editor.UI
{
    public class UiManager : IEnumerable<UiComponent>
    {
        public readonly List<UiComponent> Components = new();
        private readonly Application app;
        public readonly Config Config;

        public UiManager(Application app)
        {
            this.app = app;
            Config = app.Session.Config;
        }

        public void AddComponent(UiComponent component)
        {
            Components.Add(component);

            component.Initialize(this);
        }

        public void AddRange(IEnumerable<UiComponent> components)
        {
            Components.AddRange(components);

            foreach (UiComponent component in components)
                component.Initialize(this);
        }

        public T GetComponent<T>() where T : UiComponent
        {
            foreach (UiComponent component in Components)
            {
                if (component is T t)
                    return t;
            }
            return null;
        }

        public List<T> GetComponents<T>() where T : UiComponent
        {
            List<T> result = new();
            foreach (UiComponent component in Components)
            {
                if (component is T t)
                    result.Add(t);
            }
            return result;
        }

        public void UpdateComponents(GameTime time)
        {
            foreach (UiComponent component in Components)
            {
                if (component is IUpdateable updateable)
                    updateable.Update(time);
            }
        }

        public void RenderComponents(RenderingCall call)
        {
            foreach (UiComponent component in Components)
            {
                if (component.Call == call)
                    component.Render();
            }
        }

        internal void SaveComponents()
        {
            foreach (UiComponent component in Components)
                component.Save(Config);
        }

        public IEnumerator<UiComponent> GetEnumerator() => Components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Components.GetEnumerator();
    }
}
