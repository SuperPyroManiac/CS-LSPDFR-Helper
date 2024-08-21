namespace LSPDFRHelper.CustomTypes.SpecialTypes;

public record EnvironmentConfig(
    string BotToken,
    string DbServer,
    string DbUser,
    string DbPass,
    string DbName,
    string SchemaName,
    ulong MainServ,
    ulong LogCh,
    ulong SLogCh,
    ulong ErrLogCh
);
