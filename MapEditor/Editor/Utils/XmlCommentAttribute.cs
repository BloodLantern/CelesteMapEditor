using System;

namespace Editor.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public string Value { get; set; }

        public XmlCommentAttribute(string value) => Value = value;
    }
}
