namespace LSPDFR_Helper.CustomTypes.SpecialTypes;

/// <summary>
/// An EnvironmentConfig record can be used to change environment and Discord server dependent config settings (e.g. for development environments).
/// </summary>
/// <param name="BotToken">The token of the Discord bot that is needed in the DiscordConfiguration object when initializing the bot.</param>
/// <param name="DbServer">Database location</param>
/// <param name="DbUser">Database username</param>
/// <param name="DbPass">Database password</param>
/// <param name="DbName">Database name</param>
public record EnvironmentConfig(
    string BotToken,
    string DbServer,
    string DbUser,
    string DbPass,
    string DbName
);
