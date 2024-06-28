using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.LogTypes;
using LSPDFR_Helper.Functions.Processors.RPH;

namespace LSPDFR_Helper.CustomTypes.CacheTypes;

public class ProcessCache
{
    public DateTime Expire = DateTime.Now.AddMinutes(15);
    public DiscordMessageInteraction Interaction { get; private set; }
    public DiscordMessage OriginalMessage { get; private set; }
    public RphProcessor RphProcessor { get; set; }

    public ProcessCache(DiscordMessageInteraction interaction, DiscordMessage originalMessage, RphProcessor rphProcessor)
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        RphProcessor = rphProcessor;
    }
    
    public ProcessCache(DiscordMessageInteraction interaction, DiscordMessage originalMessage)
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
    }

    public ProcessCache Update(ProcessCache newCache)
    {
        if ((OriginalMessage ?? newCache.OriginalMessage) != null && OriginalMessage.Id != newCache.OriginalMessage.Id)
            throw new InvalidOperationException("Cannot update ProcessCache that belongs to a different message!");
        
        Interaction = newCache.Interaction ?? Interaction;
        OriginalMessage = newCache.OriginalMessage ?? OriginalMessage;
        RphProcessor = newCache.RphProcessor ?? RphProcessor;
        Expire = DateTime.Now.AddMinutes(15);
        return this;
    }

    /// <summary>
    /// Determines whether the cached data can be utilized to retrieve log analysis results for a specific log type.
    /// </summary>
    /// <param name="logType">The type of log to be analyzed (valid types: RagePluginHook, ELS, asiloader, ScriptHookVDotNet).</param>
    /// <param name="cache">The ProcessCache object to be validated.</param>
    /// <param name="attachments">If several attachments are attached to the message, the list of attachments can be passed here to check whether it contains several log files of the same type.</param>
    /// <returns>True if the cache can be used to get the log analysis results for the specified log type; false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid log type is provided.</exception>
    public static bool IsCacheUsagePossible(string logType, ProcessCache cache, List<DiscordAttachment> attachments=null)
    {
        if (cache == null) return false;

        if (attachments != null)
        {
            var countOfType = attachments.Count(attachment => attachment.FileName!.Contains(logType));
            if (countOfType > 1) return false;
        }
        switch (logType)
        {
            case "RagePluginHook":
                if (cache.RphProcessor.Log == null) return false;
                break;
            default:
                throw new ArgumentException($"Invalid log type '{logType}'.");
        }
        return true;
    }
}