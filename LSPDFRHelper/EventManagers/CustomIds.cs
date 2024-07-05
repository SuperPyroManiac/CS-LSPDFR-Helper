namespace LSPDFRHelper.EventManagers;

public record CustomIds
{
    // Msc
    public const string SelectAttachmentForAnalysis = "SelectAttachmentForAnalysis";
    public const string SelectIdForRemoval = "SelectIdForRemoval";
    
    // Editor
    public const string SelectPluginValueToEdit = "SelectPluginValueToEdit";
    public const string SelectPluginValueToFinish = "SelectPluginValueToFinish";
    public const string SelectErrorValueToEdit = "SelectErrorValueToEdit";
    public const string SelectErrorValueToFinish = "SelectErrorValueToFinish";
    public const string SelectUserValueToEdit = "SelectUserValueToEdit";
    
    // public
    public const string RequestHelp = "RequestHelp";
    public const string MarkSolved = "MarkSolved";
    public const string JoinCase = "JoinCase";
    public const string IgnoreRequest = "IgnoreRequest";
    public const string OpenCase = "OpenCase";

    // RPH log analysis events
    public const string RphGetQuickInfo = "RphGetQuickInfo";
    public const string RphGetErrorInfo = "RphGetDetailedInfo";
    public const string RphGetPluginInfo = "RphGetAdvancedInfo";
    public const string RphSendToUser = "RphQuickInfoSendToUser";

    // ELS log analysis events
    public const string ElsSendToUser = "ElsQuickInfoSendToUser";

    // ASI log analysis events
    public const string AsiSendToUser = "AsiQuickInfoSendToUser";

    // SHVDN log analysis events
    public const string ShvdnGetQuickInfo = "ShvdnGetQuickInfo";
    public const string ShvdnGetDetailedInfo = "ShvdnGetDetailedInfo";
    public const string ShvdnQuickSendToUser = "ShvdnQuickInfoSendToUser";
    public const string ShvdnDetailedSendToUser = "ShvdnDetailedSendToUser";
}
