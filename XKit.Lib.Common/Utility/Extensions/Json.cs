
using System;
using System.Text.Json;

namespace XKit.Lib.Common.Utility.Extensions {

    public static class Json {

        private static JsonSerializerOptions baseOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyProperties = true
        };

        private static JsonSerializerOptions baseOptionsPretty = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyProperties = true
        };

        public static string ToJson<T>(T obj, bool pretty = false) {
            if (obj == null) { return null; }
            return JsonSerializer.Serialize(obj, pretty ? baseOptionsPretty : baseOptions);
        }

        public static string ToJson(object obj, bool pretty = false) {
            if (obj == null) { return null; }
            return JsonSerializer.Serialize<dynamic>(obj, pretty ? baseOptionsPretty : baseOptions);
        }

        public static T FromJson<T>(string json) {
            if (json == null) { return default; }
            return JsonSerializer.Deserialize<T>(json, baseOptions);
        }

        public static object FromJson(string json, Type type) {
            if (json == null) { return null; }
            return JsonSerializer.Deserialize(json, type, baseOptions);
        }

        public static dynamic FromJson(string json) {
            return JsonSerializer.Deserialize<dynamic>(json);
        }
    }
}
