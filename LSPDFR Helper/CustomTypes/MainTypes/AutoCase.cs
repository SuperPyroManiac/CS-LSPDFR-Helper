namespace LSPDFR_Helper.CustomTypes.MainTypes;

internal class AutoCase
{
    internal string CaseId { get; set; }
    internal ulong OwnerId { get; set; }
    internal ulong ChannelId { get; set; }
    internal ulong RequestId { get; set; }
    internal bool Solved { get; set; }
    internal bool TsRequested { get; set; }
    internal DateTime CreateDate { get; set; }
    internal DateTime ExpireDate { get; set; }
}