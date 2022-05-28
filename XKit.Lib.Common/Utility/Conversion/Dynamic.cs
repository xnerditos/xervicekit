using System;
using System.Runtime.Serialization;

namespace XKit.Lib.Common.Utility.Conversion {
    public static class DynamicConversionHelper {

        public static T FromDynamic<T>(dynamic dynObject) {
            return (T)FromDynamic(dynObject, typeof(T));
        }

        public static object FromDynamic(dynamic dynObject, System.Type toType) {
            object obj = Activator.CreateInstance(toType);
            var members = FormatterServices.GetSerializableMembers(toType);
            var dynamicData = FormatterServices.GetObjectData(dynObject, members);
            FormatterServices.PopulateObjectMembers(
                obj, 
                members,
                dynamicData
            );
            return obj;
        }
    }
}