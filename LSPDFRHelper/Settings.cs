using System.Text.Encodings.Web;
using System.Text.Json;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.SpecialTypes;

namespace LSPDFRHelper;

public class Settings
{
    private const string ConfigFileName = "environment-config.json";
    public EnvironmentConfig Env { get; }

    public Settings()
    {
        Env = LoadEnvConfigFile();
    }

    public static string GetOrCreateFolder(string folder)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), folder);
        if (!Path.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    private static EnvironmentConfig LoadEnvConfigFile()
    {
        var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);

        if (!File.Exists(jsonFilePath))
        {
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            using var newFile = File.Create(jsonFilePath);
            JsonSerializer.Serialize(newFile, GetDefaultEnvConfig(), options: options);
            throw new FileNotFoundException("Environment config file could not be found. One has been created for you. Please add your bot token to the file!", jsonFilePath);
        }
        
        var jsonContent = File.ReadAllText(jsonFilePath);
        var env = JsonSerializer.Deserialize<EnvironmentConfig>(jsonContent);
        if (env == null) 
            throw new FileLoadException("Environment config file could not be loaded or parsed. If you delete the current config file and restart the bot, it will generate a new default config file with a placeholder value.", jsonFilePath);

        if (env.BotToken.Equals(GetDefaultEnvConfig().BotToken))
            throw new InvalidDataException($"Error in Environment Config: Please replace the token placeholder '{GetDefaultEnvConfig().BotToken}' in the {ConfigFileName} with an actual Discord bot token!");
        
        return env;
    }

    private static EnvironmentConfig GetDefaultEnvConfig()
    {
        return new EnvironmentConfig(
            BotToken: "INSERT_BOT_TOKEN_HERE",
            DbServer: "Example.com",
            DbUser: "Username",
            DbPass: "Password",
            DbName: "Database",
            SchemaName: "Schema",
            MainServ: 0,
            LogCh: 0,
            SLogCh: 0,
            ErrLogCh: 0
        );
    }

    public async Task<DiscordGuild> MainGuild()
    {
        return await Program.Client.GetGuildAsync(Program.BotSettings.Env.MainServ);
    }
    public async Task<DiscordChannel> BotLogs()
    {
        return await Program.Client.GetChannelAsync(Program.BotSettings.Env.LogCh);
    }
    public async Task<DiscordChannel> ServerLogs()
    {
        return await Program.Client.GetChannelAsync(Program.BotSettings.Env.SLogCh);
    }
    public async Task<DiscordChannel> ErrorLogs()
    {
        return await Program.Client.GetChannelAsync(Program.BotSettings.Env.ErrLogCh);
    }
}
