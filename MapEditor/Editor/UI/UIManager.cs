using Editor.Saved;
using System.Collections;
using System.Collections.Generic;

namespace Editor.UI
{
    public class UIManager : IEnumerable<UIComponent>
    {
        public readonly List<UIComponent> Components = new();
        private readonly Application app;
        public readonly Config Config;

        public UIManager(Application app)
        {
            this.app = app;
            Config = app.Session.Config;
        }

        public void AddComponent(UIComponent component)
        {
            Components.Add(component);

            component.Initialize(this);
        }

        public void AddRange(IEnumerable<UIComponent> components)
        {
            Components.AddRange(components);

            foreach (UIComponent component in components)
                component.Initialize(this);
        }

        public T GetComponent<T>() where T : UIComponent
        {
            foreach (UIComponent component in Components)
            {
                if (component is T t)
                    return t;
            }
            return null;
        }

        public List<T> GetComponents<T>() where T : UIComponent
        {
            List<T> result = new();
            foreach (UIComponent component in Components)
            {
                if (component is T t)
                    result.Add(t);
            }
            return result;
        }

        public void RenderComponents(RenderingCall call)
        {
            foreach (UIComponent component in Components)
            {
                if (component.Call == call)
                    component.Render();
            }
        }

        internal void SaveComponents()
        {
            foreach (UIComponent component in Components)
                component.Save(Config);
        }

        public IEnumerator<UIComponent> GetEnumerator() => Components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Components.GetEnumerator();
    }
}
