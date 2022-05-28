namespace XKit.Lib.Common.Registration {
	
	public enum HealthEnum : int {
		Dead = 0,
		Critical = 1,
		UnhealthyFailing = 2,
		UnhealthyRecovering = 3,
        Unknown = 4,        // could not be determined
		Moderate = 5,
		Healthy = 6
	}

	public enum AvailabilityEnum : int {
		Unavailable = 0,
		GoingOffLine = 1,
		ServicePaused = 2,
		UnavailableRetryLater = 3,
		Serving5 = 5,
		Serving6 = 6,
		Serving7 = 7,
		Serving8 = 8,
		Serving9 = 9
	}

	public enum RunStateEnum {
        Unknown = -1,
		Inactive = 0,
		StartingUp = 1,
		Paused = 2,
		Active = 3,
        Stale = 4,
        ShuttingDown = 5
	}

    public enum ServiceCallPatternEnum {
        FirstChance,
        SpecificHost
    }
}