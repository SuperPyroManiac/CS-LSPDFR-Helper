namespace ULSS_Helper.Objects;

/// <summary>
/// An EnvironmentConfig record can be used to change environment and Discord server dependent config settings (e.g. for development environments).
/// </summary>
/// <param name="BotToken">The token of the Discord bot that is needed in the DiscordConfiguration object when initializing the bot.</param>
/// <param name="DbFileName">The name of the local SQLite DB file.</param>
/// <param name="RphVersion">The current RPH version.</param>
/// <param name="LspdfrVersion">The current LSPDFR version.</param>
/// <param name="GtaVersion">The current GTA V version.</param>
/// <param name="ServerId">The ID of the Discord server where this bot should be working on (also known as "guild id").</param>
/// <param name="TsRoleId">The ID of the "Tech Support" role on the Discord server that should be allowed to use advanced bot commands.</param>
/// <param name="TsIconUrl">The URL of the icon/image asset that should be used in the bot's embed thumbnail fields.</param>
/// <param name="MoreInfoBtnEmojiId">The ID of the emoji that should be shown on the "More info" button when analyzing logs.</param>
/// <param name="BotAdminUserIds">A list of user IDs of the bot admins (can change the TS table in the DB and grant bot command usage access to specific users).</param>
/// <param name="BotBlacklistRoleId">The role ID of the role on the server that should be used to blacklist users from bot use.</param>
/// <param name="PublicUsageAllowedChannelIds">A list of channel IDs where the public users are allowed to use the bot commands (/checkLog, etc.).</param>
/// <param name="TsBotLogChannelId">The channel ID of the channel on the server where the bot should log everything that should be visible to users with the TS role.</param>
/// <param name="PublicBotLogChannelId">The channel ID of the channel on the server where the bot should log all actions related to public users using the bot commands (checkLog) on their own.</param>
/// <param name="PublicBotReportsChannelId">The channel ID of the channel on the server where the bot should log reports related to public users using the bot commands (checkLog) on their own.</param>
/// <param name="StaffContactChannelId">The channel ID of the "staff contact" channel on the server.</param>
/// <param name="BullyingVictims">A list of users that should be bullied by the bot with random reactions.</param>
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
