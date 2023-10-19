using System.Collections;
using System.Collections.Generic;

namespace Editor.UI
{
    public class UIManager : IEnumerable<UIComponent>
    {
        public readonly List<UIComponent> Components = new();
        private Application app;
        private Config config;

        public UIManager(Application app)
        {
            this.app = app;
            config = app.Session.Config;
        }

        public void AddComponent(UIComponent component)
        {
            Components.Add(component);

            component.Load(config);
        }

        public void AddRange(IEnumerable<UIComponent> components)
        {
            Components.AddRange(components);

            foreach (UIComponent component in components)
                component.Load(config);
        }

        public T FindComponent<T>() where T : UIComponent
        {
            foreach (UIComponent component in Components)
            {
                if (component is T t)
                    return t;
            }
            return null;
        }

        public List<T> FindComponents<T>() where T : UIComponent
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
                component.Save(config);
        }

        public IEnumerator<UIComponent> GetEnumerator() => Components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Components.GetEnumerator();
    }
}
