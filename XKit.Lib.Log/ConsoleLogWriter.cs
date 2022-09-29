using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Log {

    public class ConsoleLogWriter : ILogWriter {
        private static readonly Regex stripKeyQuotes;

        static ConsoleLogWriter() {
            stripKeyQuotes = new Regex("\"([^\"]+)\":", RegexOptions.Compiled);
        }

        public static bool PrettyOutput { get; set; } = false;
        public static bool OutputPureJson { get; set; } = false;
        
        void ILogWriter.WriteEvent(IReadOnlyLogEventEntry logEvent) {
            string json = Json.ToJson(logEvent, PrettyOutput);
            string line = OutputPureJson ? json : $">> XKIT " + stripKeyQuotes.Replace(json, " $1: ");
            Console.WriteLine(line);
            Debug.WriteLine(line);
        }
    }
}
