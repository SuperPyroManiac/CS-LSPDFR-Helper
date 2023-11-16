namespace ULSS_Helper.Objects;

public record EnvironmentConfig(
    string BotToken, 
    string DbFileName, 
    string RphVersion, 
    string LspdfrVersion, 
    string GtaVersion, 
    ulong ServerId, 
    ulong TsRoleId, 
    string TsIconUrl, 
    ulong MoreInfoBtnEmojiId, 
    List<ulong> BotAdminUserIds, 
    ulong BotBlacklistRoleId, 
    List<ulong> PublicUsageAllowedChannelIds, 
    ulong TsBotLogChannelId, 
    ulong PublicBotLogChannelId, 
    ulong PublicBotReportsChannelId, 
    ulong StaffContactChannelId, 
    List<ulong> BullyingVictims
)
{};
