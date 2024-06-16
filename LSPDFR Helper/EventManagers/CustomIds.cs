namespace LSPDFR_Helper.EventManagers;

internal record CustomIds()
{
    // Msc
    internal const string SelectAttachmentForAnalysis = "SelectAttachmentForAnalysis";
    internal const string SelectIdForRemoval = "SelectIdForRemoval";
    
    // Editor
    internal const string SelectPluginValueToEdit = "SelectPluginValueToEdit";
    internal const string SelectPluginValueToFinish = "SelectPluginValueToFinish";
    internal const string SelectErrorValueToEdit = "SelectErrorValueToEdit";
    internal const string SelectErrorValueToFinish = "SelectErrorValueToFinish";
    internal const string SelectUserValueToEdit = "SelectUserValueToEdit";
    
    // internal
    internal const string SendFeedback = "SendFeedback";
    internal const string RequestHelp = "RequestHelp";
    internal const string MarkSolved = "MarkSolved";
    internal const string JoinCase = "JoinCase";
    internal const string IgnoreRequest = "IgnoreRequest";
    internal const string OpenCase = "OpenCase";

    // RPH log analysis events
    internal const string RphGetQuickInfo = "RphGetQuickInfo";
    internal const string RphGetDetailedInfo = "RphGetDetailedInfo";
    internal const string RphGetAdvancedInfo = "RphGetAdvancedInfo";
    internal const string RphQuickSendToUser = "RphQuickInfoSendToUser";
    internal const string RphDetailedSendToUser = "RphDetailedSendToUser";

    // ELS log analysis events
    internal const string ElsGetQuickInfo = "ElsGetQuickInfo";
    internal const string ElsGetDetailedInfo = "ElsGetDetailedInfo";
    internal const string ElsQuickSendToUser = "ElsQuickInfoSendToUser";
    internal const string ElsDetailedSendToUser = "ElsDetailedSendToUser";

    // ASI log analysis events
    internal const string AsiGetQuickInfo = "AsiGetQuickInfo";
    internal const string AsiGetDetailedInfo = "AsiGetDetailedInfo";
    internal const string AsiQuickSendToUser = "AsiQuickInfoSendToUser";
    internal const string AsiDetailedSendToUser = "AsiDetailedSendToUser";

    // SHVDN log analysis events
    internal const string ShvdnGetQuickInfo = "ShvdnGetQuickInfo";
    internal const string ShvdnGetDetailedInfo = "ShvdnGetDetailedInfo";
    internal const string ShvdnQuickSendToUser = "ShvdnQuickInfoSendToUser";
    internal const string ShvdnDetailedSendToUser = "ShvdnDetailedSendToUser";
}
