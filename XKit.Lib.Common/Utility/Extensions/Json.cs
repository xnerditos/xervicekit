
using System;
using System.Text.Json;

namespace XKit.Lib.Common.Utility.Extensions {

    public static class Json {

        public static string To<T>(T obj, bool pretty = false) {
            if (obj == null) { return null; }
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions() { WriteIndented = pretty });
        }

        public static string To(object obj, bool pretty = false) {
            if (obj == null) { return null; }
            return JsonSerializer.Serialize<dynamic>(obj, new JsonSerializerOptions() { WriteIndented = pretty });
        }

        public static string ToDynamic(object obj, bool pretty = false) {
            if (obj == null) { return null; }
            return JsonSerializer.Serialize<dynamic>(obj, new JsonSerializerOptions() { WriteIndented = pretty });
        }

        public static T From<T>(string json) {
            if (json == null) { return default; }
            return JsonSerializer.Deserialize<T>(json);
        }

        public static object From(string json, Type type) {
            if (json == null) { return null; }
            return JsonSerializer.Deserialize(json, type);
        }

        public static object FromDynamic(string json) {
            return JsonSerializer.Deserialize<dynamic>(json);
        }
    }
}
