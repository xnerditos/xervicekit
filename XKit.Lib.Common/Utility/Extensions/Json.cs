
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Utility.Extensions;

public static class Json {

    private static readonly JsonSerializerOptions baseOptions;

    private static readonly JsonSerializerOptions baseOptionsPretty;

    static Json() {
        baseOptions = new(JsonSerializerDefaults.Web) {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = true,
        };

        baseOptionsPretty = new(baseOptions) {
            WriteIndented = true,
        };
    }

    public static string ToJson<T>(T obj, bool pretty = false) {
        if (obj == null) { return null; }
        
        return JsonSerializer.Serialize(obj, pretty ? baseOptionsPretty : baseOptions);
    }

    public static string ToJson(object obj, bool pretty = false) {
        if (obj == null) { return null; }
        var type = obj.GetType();
        return JsonSerializer.Serialize(obj, type, pretty ? baseOptionsPretty : baseOptions);
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
