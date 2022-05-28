using System;

namespace XKit.Lib.Common.Utility.Extensions {

    public static class TypeExtensions {

        // =====================================================================
        // Public extensios
        // =====================================================================

        public static object FromJson(this Type t, string json) {
            return Json.From(json, t);
        }
    }
}