using System;

namespace Editor.Saved.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ComparisonIgnoreAttribute : Attribute
    {
    }
}
