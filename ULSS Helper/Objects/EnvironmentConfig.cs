namespace ULSS_Helper.Objects;

/// <summary>
/// An EnvironmentConfig record can be used to change environment and Discord server dependent config settings (e.g. for development environments).
/// </summary>
/// <param name="BotToken">The token of the Discord bot that is needed in the DiscordConfiguration object when initializing the bot.</param>
/// <param name="DbServer">Database location</param>
/// <param name="DbUser">Database username</param>
/// <param name="DbPass">Database password</param>
/// <param name="DbName">Database name</param>
/// <param name="ServerId">The ID of the Discord server where this bot should be working on (also known as "guild id").</param>
/// <param name="TsRoleId">The ID of the "Tech Support" role on the Discord server that should be allowed to use advanced bot commands.</param>
/// <param name="TsIconUrl">The URL of the icon/image asset that should be used in the bot's embed thumbnail fields.</param>
/// <param name="PublicUsageAllowedChannelIds">A list of channel IDs where the public users are allowed to use the bot commands (/checkLog, etc.).</param>
/// <param name="AutoHelperChannelId">The channel that the autohelper will run in.</param>
/// <param name="SupportChannelId">The public support channel the bot will monitor.</param>
/// <param name="RequestHelpChannelId">The channel that the autohelper will request help in.</param>
/// <param name="TsBotLogChannelId">The channel ID of the channel on the server where the bot should log everything that should be visible to users with the TS role.</param>
/// <param name="PublicBotLogChannelId">The channel ID of the channel on the server where the bot should log all actions related to public users using the bot commands (checkLog) on their own.</param>
/// <param name="PublicBotReportsChannelId">The channel ID of the channel on the server where the bot should log reports related to public users using the bot commands (checkLog) on their own.</param>
/// <param name="StaffContactChannelId">The channel ID of the "staff contact" channel on the server.</param>
/// <param name="OskaruApiBaseUrl">The base url for the oskaru.net API (mainly used for text recognition in images uploaded to Discord).</param>
/// <param name="OskaruApiKey">The API access key for connecting to the oskaru.net API.</param>
public record EnvironmentConfig(
    string BotToken,
    string DbServer,
    string DbUser,
    string DbPass,
    string DbName,
    ulong ServerId,
    ulong TsRoleId,
    string TsIconUrl,
    List<ulong> PublicUsageAllowedChannelIds,
    ulong AutoHelperChannelId,
    ulong SupportChannelId,
    ulong RequestHelpChannelId,
    ulong TsBotLogChannelId,
    ulong PublicBotLogChannelId,
    ulong PublicBotReportsChannelId,
    ulong StaffContactChannelId,
    string OskaruApiBaseUrl,
    string OskaruApiKey
);
