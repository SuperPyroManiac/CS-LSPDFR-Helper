﻿using ULSS_Helper.Objects;
using System.Text.Json;

namespace ULSS_Helper;

internal class Settings
{
    private const string ConfigFileName = "environment-config.json";
    internal string DbPath { get; }
    internal string DbLocation  { get; }
    internal EnvironmentConfig Env { get; }

    internal Settings()
    {
        Env = LoadEnvConfigFile();
        DbPath = Path.Combine(Directory.GetCurrentDirectory(), Env.DbFileName);
        DbLocation = $"Data Source={DbPath};Version=3;";
    }
    
    internal static string GenerateNewFilePath(FileType fileType)
    {
        string fileName;
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd_HH-mm-ss-fff");

        switch(fileType)
        {
            case FileType.RPH_LOG:
                fileName = $"RagePluginHook_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder("RPHLogs"), fileName);

            case FileType.ELS_LOG:
                fileName = $"ELS_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder("ELSLogs"), fileName);

            case FileType.ASI_LOG:
                fileName = $"asiloader_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder("ASILogs"), fileName);

            case FileType.SHVDN_LOG:
                fileName = $"ScriptHookVDotNet_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder( "SHVDNLogs"), fileName);

            case FileType.DB_BACKUP:
                fileName = $"ULSSDB_{formattedDateTime}.db";
                return Path.Combine(GetOrCreateFolder( "Backups"), fileName);

            default:
                throw new ArgumentException("Invalid FileType!");
        }
    }

    private static string GetOrCreateFolder(string folder)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), folder);
        if (!Path.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    private static EnvironmentConfig LoadEnvConfigFile()
    {
        string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);

        // if file doesn't exist, create one with the default config (including a placeholder for the token)
        if (!File.Exists(jsonFilePath))
        {
            FileStream newFile = File.Create(jsonFilePath);
            JsonSerializer.Serialize(newFile, GetDefaultEnvConfig(), options: new JsonSerializerOptions() { WriteIndented = true });
            throw new FileNotFoundException($"Environment config file could not be found. One has been created for you. Please add your bot token to the file!", jsonFilePath);
        }
        
        string jsonContent = File.ReadAllText(jsonFilePath);
        EnvironmentConfig? env = JsonSerializer.Deserialize<EnvironmentConfig>(jsonContent);
        if (env == null) 
            throw new FileLoadException("Environment config file could not be loaded or parsed. If you delete the current config file and restart the bot, it will generate a new default config file with a placeholder value.", jsonFilePath);

        // check if the token placeholder was replaced
        if (env.BotToken.Equals(GetDefaultEnvConfig().BotToken))
            throw new InvalidDataException($"Error in Environment Config: Please replace the token placeholder '{GetDefaultEnvConfig().BotToken}' in the {ConfigFileName} with an actual Discord bot token!");

        // only allow this bot on specific Discord servers
        List<ulong> serverIdWhitelist = new(){449706194140135444, 1166534357792600155};
        if (!serverIdWhitelist.Any(whitelistId => whitelistId == env.ServerId))
            throw new InvalidDataException($"Error in Environment Config: You are not allowed to use this bot on the Discord server with ID '{env.ServerId}'!");
        
        Console.WriteLine($"Successfully loaded environment config from '{ConfigFileName}'!");
        return env;
    }

    private static EnvironmentConfig GetDefaultEnvConfig()
    {
        return new EnvironmentConfig(
            BotToken: "INSERT_BOT_TOKEN_HERE",
            DbFileName: "ULSSDB.db",
            RphVersion: "1.106.1330.16514",
            LspdfrVersion: "0.4.8678.25591",
            GtaVersion: "1.0.3028.0",
            ServerId: 449706194140135444,
            TsRoleId: 517568233360982017,
            TsIconUrl: "https://cdn.discordapp.com/role-icons/517568233360982017/b69077cfafb6856a0752c863e1bb87f0.webp?size=128&quality=lossless",
            MoreInfoBtnEmojiId: 723417756938010646,
            BotAdminUserIds: new(){339550607847194624},
            BotBlacklistRoleId: 1134534059771572356,
            PublicUsageAllowedChannelIds: new(){672541961969729540, 692254906752696332},
            TsBotLogChannelId: 1173304071084585050,
            PublicBotLogChannelId: 1173304117557477456,
            PublicBotReportsChannelId: 547311030477520896,
            StaffContactChannelId: 693303741071228938,
            BullyingVictims: new(){478591527321337857, 614191277528973428}
        );
    } 
}
