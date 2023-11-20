using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Editor.Saved
{
    /// <summary>
    /// Base class for all configuration classes. These classes are used to save and load
    /// configuration data and they should only define fields that can be serialized.
    /// This class is not intended to be used directly therefore being declared as abstract.
    /// It doesn't have the <see cref="SerializableAttribute"/> because it couldn't be inherited
    /// otherwise. However, all classes derived from this one should have this attribute.
    /// </summary>
    public abstract class ConfigBase : ICloneable
    {
        /// <summary>
        /// Makes a deep copy of the current Config object. Any class derived from <see cref="ConfigBase"/>
        /// need to implement this method with a simple call to <see cref="CloneFields{T}"/> with the
        /// derived class as the type parameter.
        /// </summary>
        /// <returns>A new instance of this class with the same values as this one.</returns>
        public abstract object Clone();

        protected T CloneFields<T>() where T : ConfigBase, new()
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
    }
}
