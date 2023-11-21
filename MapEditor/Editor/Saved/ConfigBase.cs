using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Editor.Saved
{
    /// <summary>
    /// Base class for all configuration classes. These classes are used to save and load
    /// configuration data, they should only define fields that can be serialized.
    /// This class is not intended to be used directly therefore being declared as abstract.
    /// It doesn't have the <see cref="SerializableAttribute"/> because it couldn't be inherited
    /// otherwise. However, all classes derived from this one should have this attribute.
    /// </summary>
    public abstract class ConfigBase : ICloneable, IEquatable<ConfigBase>
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

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj) => obj is ConfigBase configBase && Equals(configBase);

        public bool Equals(ConfigBase other)
        {
            Type type = GetType();

            if (type != other.GetType())
                return false;

            if (this == other)
                return true;

            foreach (FieldInfo field in type.GetFields())
            {
                if (field.IsLiteral)
                    continue;

                Type fieldType = field.FieldType;
                if (fieldType.IsClass)
                {
                    object value = field.GetValue(this);
                    object otherValue = field.GetValue(other);
                    if (value is ConfigBase configBase && otherValue is ConfigBase otherConfigBase)
                    {
                        if (!configBase.Equals(otherConfigBase)) return false;
                    }
                    else if (value is List<string> list && otherValue is List<string> otherList)
                    {
                        if (!Calc.DeepEqualsList(list, otherList)) return false;
                    }
                    else if (value is ICloneable cloneable && otherValue is ICloneable otherCloneable)
                    {
                        if (!cloneable.Equals(otherCloneable)) return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected bool Equals<T>(T other) where T : ConfigBase
        {
            if (GetType() != other.GetType())
                return false;

            if (this == other)
                return true;

            foreach (FieldInfo field in typeof(T).GetFields())
            {
                if (field.IsLiteral)
                    continue;

                if (field.FieldType.IsClass)
                {
                    object value = field.GetValue(this);
                    object otherValue = field.GetValue(other);
                    if (value is ConfigBase configBase && otherValue is ConfigBase otherConfigBase)
                    {
                        if (!configBase.Equals(otherConfigBase)) return false;
                    }
                    else if (value is List<string> list && otherValue is List<string> otherList)
                    {
                        if (!Calc.DeepEqualsList(list, otherList)) return false;
                    }
                    else if (value is ICloneable cloneable && otherValue is ICloneable otherCloneable)
                    {
                        if (!cloneable.Equals(otherCloneable)) return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
