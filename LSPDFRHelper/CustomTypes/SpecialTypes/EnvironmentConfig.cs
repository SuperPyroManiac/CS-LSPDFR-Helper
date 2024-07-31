namespace LSPDFRHelper.CustomTypes.SpecialTypes;

public record EnvironmentConfig(
    string BotToken,
    string DbServer,
    string DbUser,
    string DbPass,
    string DbName,
    ulong MainServ,
    ulong LogCh,
    ulong SLogCh,
    ulong ErrLogCh
);
