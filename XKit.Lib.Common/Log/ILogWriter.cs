namespace XKit.Lib.Common.Log {

    public interface ILogWriter {
        void WriteEvent(IReadOnlyLogEventEntry logEvent);
    }
}
