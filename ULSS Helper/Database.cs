﻿using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using MySqlConnector;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

public class Database
{
    private static readonly string ConnStr = $"Server={Program.Settings.Env.DbServer};User ID={Program.Settings.Env.DbUser};Password={Program.Settings.Env.DbPass};Database={Program.Settings.Env.DbName}";

    public static bool AutoHelperStatus(string state = null)
    {
        if (state == null)
        {
            try
            {
                using IDbConnection cnn = new MySqlConnection(ConnStr);
                var output = cnn.Query<string>("select Value from GlobalValues where Name = 'AHStatus'");
                cnn.Close();
                return output.First() == "1";
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
                throw;
            }
        }
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            cnn.Execute($"UPDATE GlobalValues SET Value = '{state}' WHERE Name = 'AHStatus'");
            var output = state == "1";
            cnn.Close();
            return output;
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    public static List<Plugin> LoadPlugins()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<Plugin>("select * from Plugin");
            cnn.Close();
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }

    public static Plugin GetPlugin(string pluginName)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<Plugin>($"select * from Plugin where Name='{pluginName}'");
            output = output.ToList();
            cnn.Close();
            if (output.Count() == 1) 
            {
                return output.First();
            }
            if (output.Count() > 1)
            {
                throw new Exception($"GetPlugin unexpectedly returned more than one result for the plugin name '{pluginName}'!");
            }
            return null;
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    public static List<Plugin> FindPlugins(string name = null, string dName = null, string id = null, State? state = null, string description = null, bool? exactMatch = false)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            List<string> conditions = [];
            var comparisonOperator = " = '";
            var endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }

            if (name != null) conditions.Add("Name" + comparisonOperator + name + endOfComparison);
            if (dName != null) conditions.Add("DName" + comparisonOperator + dName + endOfComparison);
            if (id != null) conditions.Add("ID" + comparisonOperator + id + endOfComparison);
            if (state != null) conditions.Add("State" + comparisonOperator + state + endOfComparison);
            if (description != null) conditions.Add("Description" + comparisonOperator + description + endOfComparison);

            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            var conditionsString = string.Join(" and ", conditions);
            var output = cnn.Query<Plugin>($"select * from Plugin where {conditionsString}");

            cnn.Close();
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
        catch (InvalidDataException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }

    public static List<Error> LoadErrors()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<Error>("select * from Error");
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }

    public static Error GetError(string errorId)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<Error>($"select * from Error where ID='{errorId}'");
            output = output.ToList();
            cnn.Close();
            if (output.Count() == 1) 
            {
                return output.First();
            }
            if (output.Count() > 1)
            {
                throw new Exception($"GetError unexpectedly returned more than one result for the error ID '{errorId}'!");
            }
            return null;
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }

    public static List<Error> FindErrors(string id, string regex, string solution, string description, Level? level, bool? exactMatch = false)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            List<string> conditions = [];
            var comparisonOperator = " = '";
            var endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }

            if (id != null) conditions.Add("ID" + comparisonOperator + id + endOfComparison);
            if (regex != null) conditions.Add("Regex" + comparisonOperator + regex + endOfComparison);
            if (solution != null) conditions.Add("Solution" + comparisonOperator + solution + endOfComparison);
            if (description != null) conditions.Add("Description" + comparisonOperator + description + endOfComparison);
            conditions.Add("Level" + comparisonOperator + level + endOfComparison);
            
            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            var conditionsString = string.Join(" and ", conditions);
            var output = cnn.Query<Error>($"select * from Error where {conditionsString}", new DynamicParameters());

            cnn.Close();
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
        catch (InvalidDataException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }

    public static async void AddPlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Plugin (Name, DName, Version, EAVersion, ID, State, Description, Link) VALUES (@Name, @DName, @Version, @EAVersion, @ID, @State, @Description, @Link)", plugin);
            Program.Cache.UpdatePlugins(LoadPlugins());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static long AddError(Error error)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            cnn.Open();
            cnn.Execute("insert into Error (Regex, Solution, Description, Level) VALUES (@Regex, @Solution, @Description, @Level)", error);
            var id = cnn.ExecuteScalar("SELECT LAST_INSERT_ID();")!;
            cnn.Close();
            Task.Run(() => Program.Cache.UpdateErrors(LoadErrors())).GetAwaiter();
            return long.Parse(id.ToString()!);
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    public static async void EditPlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Plugin SET Name = @Name, DName = @DName, Version = @Version, EAVersion = @EAVersion, ID = @ID, State = @State, Description = @Description, Link = @Link WHERE Name = (@Name)", plugin);
            Program.Cache.UpdatePlugins(LoadPlugins());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static async void EditError(Error error)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Error SET Regex = @Regex, Solution = @Solution, Description = @Description, Level = @Level WHERE ID = (@ID)", error);
            Program.Cache.UpdateErrors(LoadErrors());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    public static async void DeletePlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from Plugin where Name = (@Name)", plugin);
            Program.Cache.UpdatePlugins(LoadPlugins());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static async void DeleteError(Error error)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from Error where ID = (@ID)", error);
            Program.Cache.UpdateErrors(LoadErrors());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static List<DiscordUser> LoadUsers()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<DiscordUser>("select * from Users");
            cnn.Close();
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    
    public static async void AddUser(DiscordUser user)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Users (UID, Username, BotEditor, BotAdmin, Blocked, LogPath) VALUES (@UID, @Username, @BotEditor, @BotAdmin, @Blocked, @LogPath)", user);
            Program.Cache.UpdateUsers(LoadUsers());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static async void EditUser(DiscordUser user)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Users SET UID = @UID, Username = @Username, BotEditor = @BotEditor, BotAdmin = @BotAdmin, Blocked = @Blocked, LogPath = @LogPath WHERE UID = @UID", user);
            Program.Cache.UpdateUsers(LoadUsers());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static List<AutoCase> LoadCases()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<AutoCase>("select * from Cases");
            cnn.Close();
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    
    public static async Task AddCase(AutoCase autocase)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Cases (CaseID, OwnerID, ChannelID, Solved, TsRequested, RequestID, CreateDate, ExpireDate) VALUES (@CaseID, @OwnerID, @ChannelID, @Solved, @TsRequested, @RequestID, @CreateDate, @ExpireDate)", autocase);
            Program.Cache.UpdateCases(LoadCases());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static async Task EditCase(AutoCase autocase)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Cases SET CaseID = @CaseID, OwnerID = @OwnerID, ChannelID = @ChannelID, Solved = @Solved, TsRequested = @TsRequested, RequestID = @RequestID, CreateDate = @CreateDate, ExpireDate = @ExpireDate WHERE CaseID = (@CaseID)", autocase);
            Program.Cache.UpdateCases(LoadCases());
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    public static async void UpdatePluginVersions()
    {
        HttpClient webClient = new();
	    var plugins = LoadPlugins();
        foreach (var plugin in plugins)
        {
            try
            {
                if (plugin.ID == "0" || string.IsNullOrEmpty(plugin.ID) || plugin.State == "LIB" || plugin.State == "IGNORE") continue;
                Thread.Sleep(3500);

                var onlineVersion = await webClient.GetStringAsync(
                    $"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={plugin.ID}&textOnly=1");
                onlineVersion = onlineVersion.Split(" ")[0].Trim();
                onlineVersion = Regex.Replace(onlineVersion, "[^0-9.]", "");

                var onlineVersionSplit = onlineVersion.Split(".");
                if (onlineVersionSplit.Length == 2) onlineVersion += ".0.0";
                if (onlineVersionSplit.Length == 3) onlineVersion += ".0";

                try
                {
                    if (plugin.Version == onlineVersion) continue;
                    Console.WriteLine($"Updating Plugin {plugin.Name} from {plugin.Version} to {onlineVersion}");
                    if (string.IsNullOrEmpty(plugin.EAVersion) || plugin.EAVersion == "0")
                        await Logging.SendLog(0, 0, BasicEmbeds.Info($"__Updating Plugin!__\r\n>>> {plugin.Name} from `{plugin.Version}` to `{onlineVersion}`", true),
                            false);
                    else
                        await Logging.SendLog(0, 0,
                            BasicEmbeds.Warning($"__Updating Plugin!__\r\n>>> {plugin.Name} from `{plugin.Version}` to `{onlineVersion}`\r\nThis plugin has an EA version of `{plugin.EAVersion}` please double check it now!", true), false);
                    using IDbConnection cnn = new MySqlConnection(ConnStr);
                    await cnn.ExecuteAsync(
                        $"UPDATE Plugin SET Version = '{onlineVersion}' WHERE Name = '{plugin.Name}';");
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e);
                    Logging.ErrLog($"SQL Issue:\r\n {e}").GetAwaiter();
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"{plugin.Name} skipped.\r\n{e}");
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine($"{plugin.Name} skipped.\r\n{e}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Logging.ErrLog($"Version Updater Exception:\r\n {e}");
            }
        }
        Program.Cache.UpdatePlugins(LoadPlugins());
    }
}