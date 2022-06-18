using System;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Log {

    public class ConsoleLogWriter : ILogWriter {
        public static bool PrettyOutput { get; set; } = false;

        void ILogWriter.WriteEvent(IReadOnlyLogEventEntry logEvent) {                
            Console.WriteLine(Json.To(logEvent, PrettyOutput));
            // Console.Write($"* {entry.Timestamp.ToString("yyyy/MM/dd HH:mm:ss:FFF")} LOG:{entry.EventTypeName} |jobid={entry.LogJobEntryId} |id={entry.LogEventEntryId}");
            // if (entry.Code != null) { Console.Write($" |code={entry.Code}"); }
            // if (entry.Message != null) { Console.Write($" |message={entry.Message}"); }
            // entry.Attributes?.ForEach(a => {
            //     if (a != null) {
            //         var s = a.Value == null ? "" : a.Value.ToString();
            //         var len = s.Length < 128 ? s.Length : 128;
            //         Console.Write($" |{a.Name}={s.Substring(0, len)}");
            //     }
            // });
            // if (job.CorrelationId != null) { Console.Write(" |correlationid: " + job.CorrelationId); }
            // Console.WriteLine();            
        }
    }
}
