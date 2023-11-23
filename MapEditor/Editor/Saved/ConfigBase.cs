using System;
using System.Collections.Generic;
using System.Reflection;

namespace Editor.Saved
{
    /// <summary>
    /// Base class for all configuration classes. These classes are used to save and load
    /// configuration data, they should define fields that can be serialized.
    /// This class is not intended to be used directly and is therefore declared as abstract.
    /// It doesn't have the <see cref="SerializableAttribute"/> because it couldn't be inherited
    /// otherwise. However, all classes derived from this one should have this attribute.
    /// </summary>
    public abstract class ConfigBase : ICloneable
    {
        /// <summary>
        /// Makes a deep copy of the current Config object. Any class derived from <see cref="ConfigBase"/>
        /// need to implement this method with a simple call to <see cref="Clone{T}"/> with the
        /// derived class as the type parameter.
        /// </summary>
        /// <returns>A new instance of this class with the same values as this one.</returns>
        public abstract object Clone();

        protected T Clone<T>() where T : ConfigBase, new()
        {
            T clone = new();

            foreach (FieldInfo field in typeof(T).GetFields())
            {
                if (field.IsLiteral)
                    continue;

                if (field.FieldType.IsClass)
                {
                    object value = field.GetValue(this);

                    if (value is ConfigBase configBase)
                        field.SetValue(clone, configBase.Clone());
                    else if (value is List<string> list)
                        field.SetValue(clone, Calc.DeepCloneList(list));
                    else if (value is ICloneable cloneable)
                        field.SetValue(clone, cloneable.Clone());
                    else
                        throw new InvalidOperationException($"Field {field.Name} of type {field.FieldType.Name} is not cloneable.");
                }
                else
                {
                    field.SetValue(clone, field.GetValue(this));
                }
            }

            return clone;
        }

        /// <summary>
        /// Sets all this config's field values to those of the other config.
        /// </summary>
        /// <param name="otherConfig">The config to copy the values from.</param>
        /// <exception cref="ArgumentException">If the other config is not of the same type as the current one.</exception>
        public void CopyFrom(ConfigBase otherConfig)
        {
            Type type = GetType();

            if (type != otherConfig.GetType())
                throw new ArgumentException("Cannot copy from different types.");

            foreach (FieldInfo field in type.GetFields())
            {
                if (!field.IsLiteral)
                    field.SetValue(this, field.GetValue(otherConfig));
            }
        }

        /// <summary>
        /// Called when a new config is created, e.g. if no existing config file was found.
        /// </summary>
        internal virtual void FirstTimeSetup() { }
    }
}
