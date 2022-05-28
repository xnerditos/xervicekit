namespace XKit.Lib.Common.Log {
	
	public static class CommonAttributeNames {
		public const string CallerFileName = nameof(CallerFileName);
		public const string CallerMemberName = nameof(CallerMemberName);
		public const string CallerLineNumber = nameof(CallerLineNumber);
		public const string CallerFileHash = nameof(CallerFileHash);
		public const string StackTrace = nameof(StackTrace);
        public const string JobReplayStrategy = nameof(JobReplayStrategy);
        public const string JobReplayTargetId = nameof(JobReplayTargetId);
        public const string JobWorkItem = nameof(JobWorkItem);
        public const string JobEndResultData = nameof(JobEndResultData);
        public const string JobReplayData = nameof(JobReplayData);
    }
}