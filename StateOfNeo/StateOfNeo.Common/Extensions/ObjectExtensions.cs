using System;
using System.Reflection;

namespace StateOfNeo.Common
{
    public static class ObjectExtensions
    {
        public static T GetInstanceField<T>(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return (T)field.GetValue(instance);
        }
    }
}
