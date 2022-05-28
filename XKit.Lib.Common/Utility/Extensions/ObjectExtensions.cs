using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using Force.DeepCloner;

namespace XKit.Lib.Common.Utility.Extensions {

    public static class ObjectExtensions {

        //private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        // =====================================================================
        // Public extensios
        // =====================================================================
        // public static bool IsPrimitive(this Type type) {
        //     if (type == typeof(String)) {
        //         return true;
        //     }
        //     return (type.IsValueType & type.IsPrimitive);
        // }

        public static Object DeepCopy(this Object originalObject) {
            //return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
            return originalObject.DeepClone();
        }

        public static T DeepCopy<T>(this T original) {
            return original.DeepClone();
        }

        public static string ToJson(this object obj, bool pretty = false) {
            return Json.ToDynamic(obj, pretty);
        }

        public static string ToJson<T>(this T obj, bool pretty = false) {
            return Json.To<T>(obj, pretty);
        }

        public static string ToJsonDynamic<T>(this T obj, bool pretty = false) {
            return Json.ToDynamic(obj, pretty);
        }

        public static T FromJson<T>(this string s) {
            return Json.From<T>(s);
        }

        public static object FromJsonDynamic(this string s) {
            return Json.FromDynamic(s);
        }

        public static Dictionary<string, object> FieldsToDictionary(this object obj) {
            var dictionary = new Dictionary<string, object>();
            obj = obj ?? new object();
            foreach(var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                dictionary[field.Name] = field.GetValue(obj);
            }
            foreach(var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                dictionary[prop.Name] = prop.GetValue(obj);
            }
            return dictionary;
        }

        public static dynamic ToDynamic(this object value) {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public static dynamic ToCaseInsensitiveDynamic(this object value) {
            IDictionary<string, object> dynObj = new CaseInsensitiveDynamicObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                dynObj.Add(property.Name, property.GetValue(value));

            return dynObj as DynamicObject;
        }

        // =====================================================================
        // private helpers
        // =====================================================================

        // private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited) {
        //     if (originalObject == null) {
        //         return null;
        //     }
        //     var typeToReflect = originalObject.GetType();
        //     if (IsPrimitive(typeToReflect)) {
        //         return originalObject;
        //     }
        //     if (visited.ContainsKey(originalObject)) {
        //         return visited[originalObject];
        //     }
        //     if (typeof(Delegate).IsAssignableFrom(typeToReflect)) {
        //         return null;
        //     }
        //     var cloneObject = CloneMethod.Invoke(originalObject, null);
        //     if (typeToReflect.IsArray) {
        //         var arrayType = typeToReflect.GetElementType();
        //         if (IsPrimitive(arrayType) == false) {
        //             Array clonedArray = (Array) cloneObject;
        //             clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
        //         }
        //     }
        //     visited.Add(originalObject, cloneObject);
        //     CopyFields(originalObject, visited, cloneObject, typeToReflect);
        //     RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
        //     return cloneObject;
        // }

        // private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect) {
        //     if (typeToReflect.BaseType != null && typeToReflect.BaseType != typeof(object)) {
        //         RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
        //         CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        //     }
        // }

        // private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null) {
        //     foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags)) {
        //         if (filter != null && filter(fieldInfo) == false) { continue; }
        //         if (IsPrimitive(fieldInfo.FieldType)) { continue; }
        //         var originalFieldValue = fieldInfo.GetValue(originalObject);
        //         var clonedFieldValue = InternalCopy(originalFieldValue, visited);
        //         fieldInfo.SetValue(cloneObject, clonedFieldValue);
        //     }
        // }
    }

    // public class ReferenceEqualityComparer : EqualityComparer<Object> {
    //     public override bool Equals(object x, object y) {
    //         return ReferenceEquals(x, y);
    //     }
    //     public override int GetHashCode(object obj) {
    //         if (obj == null) { return 0; }
    //         return obj.GetHashCode();
    //     }
    // }
}