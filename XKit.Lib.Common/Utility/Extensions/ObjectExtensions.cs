using System;
using System.Collections;
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
            return Json.ToJson(obj, pretty);
        }

        public static string ToJson<T>(this T obj, bool pretty = false) {
            return Json.ToJson<T>(obj, pretty);
        }

        public static T FromJson<T>(this string s) {
            return Json.FromJson<T>(s);
        }

        public static dynamic FromJson(this string s) {
            return Json.FromJson(s);
        }

        public static Dictionary<string, object> FieldsToDictionary(this object obj) {
            var dictionary = new Dictionary<string, object>();
            obj = obj ?? new object();
            foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                dictionary[field.Name] = field.GetValue(obj);
            }
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                dictionary[prop.Name] = prop.GetValue(obj);
            }
            return dictionary;
        }

        public static dynamic ToDynamic(this object value) {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType())) {
                expando.Add(property.Name, property.GetValue(value));
            }
            return expando as ExpandoObject;
        }

        public static dynamic GraphToDynamic(this object obj) {
            var properties = obj.GetType().GetProperties();
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var property in properties) {
                var value = GetValueOrExpandoObject(obj, property);
                expando.Add(property.Name, value);
            }
            return (ExpandoObject)expando;
        }

        private static object GetValueOrExpandoObject(object @object, PropertyInfo property) {
            var value = property.GetValue(@object);
            if (value == null) return null;

            var valueType = value.GetType();
            if (valueType.IsValueType || value is string) return value;

            if (value is IEnumerable enumerable) return ToExpandoCollection(enumerable);

            return GraphToDynamic(value);
        }

        private static IEnumerable<ExpandoObject> ToExpandoCollection(IEnumerable enumerable) {
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext()) {
                yield return GraphToDynamic(enumerator.Current);
            }
        }

        // public static dynamic ToDynamic(this obj value) {
        //     IDictionary<string, obj> expando = new ExpandoObject();

        //     foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
        //         expando.Add(property.Name, property.GetValue(value));

        //     return expando as ExpandoObject;
        // }

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

        // private static void RecursiveCopyBaseTypePrivateFields(obj originalObject, IDictionary<obj, obj> visited, obj cloneObject, Type typeToReflect) {
        //     if (typeToReflect.BaseType != null && typeToReflect.BaseType != typeof(obj)) {
        //         RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
        //         CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        //     }
        // }

        // private static void CopyFields(obj originalObject, IDictionary<obj, obj> visited, obj cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null) {
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
    //     public override bool Equals(obj x, obj y) {
    //         return ReferenceEquals(x, y);
    //     }
    //     public override int GetHashCode(obj obj) {
    //         if (obj == null) { return 0; }
    //         return obj.GetHashCode();
    //     }
    // }
}
