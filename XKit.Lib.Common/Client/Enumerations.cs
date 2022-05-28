namespace XKit.Lib.Common.Client {
    public enum ServiceClientErrorHandling {
        DoNothing,
        LogInfo,
        LogWarning,
        LogError,
        LogFatality,
        ThrowException
    }
}
