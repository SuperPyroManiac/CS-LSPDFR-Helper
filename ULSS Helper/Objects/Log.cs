namespace ULSS_Helper.Objects;

public class Log
{
    public ulong MsgId { get; set; }
    public string DownloadLink { get; set; }
    public string ElapsedTime { get; set; }
    public DateTime AnalysisStartedAt { get; }
    public DateTime AnalysisCompletedAt { get; set; }
    public static readonly TimeSpan AnalysisRestartCooldown = TimeSpan.FromMinutes(1); // the default minimum age of a log analysis object before it can be overwritten by a new analysis for the same log.

    public Log()
    {
        AnalysisStartedAt = DateTime.Now;
        AnalysisCompletedAt = AnalysisStartedAt; // this is only a fallback default value, it should actually never keep this value
    }

    public bool AnalysisHasExpired(TimeSpan customMaxAge=default)
    {
        // if the input parameter maxAge was not used when calling this method, use the default cooldown period (AnalysisRestartCooldown). Otherwise use the input parameter's value. 
        TimeSpan maxAge = customMaxAge == default ? AnalysisRestartCooldown : customMaxAge;
        return (DateTime.Now - AnalysisCompletedAt) > maxAge;
    }
}