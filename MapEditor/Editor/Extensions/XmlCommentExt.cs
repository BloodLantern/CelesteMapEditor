using Editor.Utils;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Editor.Extensions
{
    public static class XmlCommentExt
    {
        const string XmlCommentPropertyPostfix = "XmlComment";

        static XmlCommentAttribute GetXmlCommentAttribute(this Type type, string memberName)
        {
            var member = type.GetProperty(memberName);
            if (member == null)
                return null;
            var attr = member.GetCustomAttribute<XmlCommentAttribute>();
            return attr;
        }

        public static XmlComment GetXmlComment(this Type type, [CallerMemberName] string memberName = "")
        {
            var attr = GetXmlCommentAttribute(type, memberName);
            if (attr == null)
            {
                if (memberName.EndsWith(XmlCommentPropertyPostfix))
                    attr = GetXmlCommentAttribute(type, memberName.Substring(0, memberName.Length - XmlCommentPropertyPostfix.Length));
            }
            if (attr == null || string.IsNullOrEmpty(attr.Value))
                return null;
            return new XmlDocument().CreateComment(attr.Value);
        }
    }
}
