namespace ZiziBot.Application.Common.Enums;

public enum MemberMuteDurationStep
{
    FiveMinutes = 5, // 5 minute
    TwoHours = 2 * 60, // 2 hour
    EighthHours = 8 * 60, // 8 hours
    Maxed = 366 * 24 * 60 // 1 year
}

public class MemberMuteDuration
{
    public static MemberMuteDurationStep[] DurationSteps => Enum.GetValues<MemberMuteDurationStep>();
    public static TimeSpan Select(int step) => TimeSpan.FromMinutes((double)DurationSteps.ElementAtOrDefault(step));
}