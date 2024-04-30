using System.Data;
using MySqlConnector;
using System.Net;
using Dapper;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper;

internal class Database
{
    private static readonly string ConnStr = $"Server={Program.Settings.Env.DbServer};User ID={Program.Settings.Env.DbUser};Password={Program.Settings.Env.DbPass};Database={Program.Settings.Env.DbName}";

    internal static List<Plugin> LoadPlugins()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<Plugin>("select * from Plugin");
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }

    internal static Plugin GetPlugin(string pluginName)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<Plugin>($"select * from Plugin where Name='{pluginName}'");
            output = output.ToList();
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
    
    internal static List<Plugin> FindPlugins(string name = null, string dName = null, string id = null, State? state = null, string description = null, bool? exactMatch = false)
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

    internal static List<Error> LoadErrors()
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

    internal static Error GetError(string errorId)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<Error>($"select * from Error where ID='{errorId}'");
            output = output.ToList();
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

    internal static List<Error> FindErrors(string id, string regex, string solution, string description, Level? level, bool? exactMatch = false)
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

    internal static async void AddPlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Plugin (Name, DName, Version, EAVersion, ID, State, Description, Link) VALUES (@Name, @DName, @Version, @EAVersion, @ID, @State, @Description, @Link)", plugin);
            await Task.Run(() => Program.Cache.UpdatePlugins(LoadPlugins()));
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static long AddError(Error error)
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
    
    internal static async void EditPlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Plugin SET Name = @Name, DName = @DName, Version = @Version, EAVersion = @EAVersion, ID = @ID, State = @State, Description = @Description, Link = @Link WHERE Name = (@Name)", plugin);
            await Task.Run(() => Program.Cache.UpdatePlugins(LoadPlugins()));
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static async void EditError(Error error)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Error SET Regex = @Regex, Solution = @Solution, Description = @Description, Level = @Level WHERE ID = (@ID)", error);
            await Task.Run(() => Program.Cache.UpdateErrors(LoadErrors()));
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    internal static async void DeletePlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from Plugin where Name = (@Name)", plugin);
            await Task.Run(() => Program.Cache.UpdatePlugins(LoadPlugins()));
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static async void DeleteError(Error error)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from Error where ID = (@ID)", error);
            await Task.Run(() => Program.Cache.UpdateErrors(LoadErrors()));
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static List<DiscordUser> LoadUsers()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<DiscordUser>("select * from Users");
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    
    internal static async void AddUser(DiscordUser user)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Users (UID, Username, BotEditor, BotAdmin, Bully, Blocked, LogPath) VALUES (@UID, @Username, @BotEditor, @BotAdmin, @Bully, @Blocked, @LogPath)", user);
            await Task.Run(() => Program.Cache.UpdateUsers(LoadUsers()));
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static async void EditUser(DiscordUser user)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Users SET UID = @UID, Username = @Username, BotEditor = @BotEditor, BotAdmin = @BotAdmin, Bully = @Bully, Blocked = @Blocked, LogPath = @LogPath WHERE UID = @UID", user);
            await Task.Run(() => Program.Cache.UpdateUsers(LoadUsers()));
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static List<AutoCase> LoadCases()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<AutoCase>("select * from Cases");
            return output.ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    
    internal static async void AddCase(AutoCase autocase)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Cases (CaseID, OwnerID, ChannelID, Solved, Timer, TsRequested, RequestID) VALUES (@CaseID, @OwnerID, @ChannelID, @Solved, @Timer, @TsRequested, @RequestID)", autocase);
            await Task.Run(() => Program.Cache.UpdateCases(LoadCases()));
            await Task.Run(CaseMonitor.UpdateMonitor);
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static async void EditCase(AutoCase autocase)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Cases SET CaseID = @CaseID, OwnerID = @OwnerID, ChannelID = @ChannelID, Solved = @Solved, Timer = @Timer, TsRequested = @TsRequested, RequestID = @RequestID WHERE CaseID = (@CaseID)", autocase);
            await Task.Run(() => Program.Cache.UpdateCases(LoadCases()));
            await Task.Run(CaseMonitor.UpdateMonitor);
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static async void UpdatePluginVersions()
    {
        #pragma warning disable SYSLIB0014
	    var webClient = new WebClient();
	    var plugins = LoadPlugins();
        foreach (var plugin in plugins)
        {
            try
            {
                if (plugin.ID == "0" || string.IsNullOrEmpty(plugin.ID) || plugin.State == "EXTERNAL") continue;
                
                string onlineVersion;
                try
                {
                    onlineVersion = webClient.DownloadString($"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={plugin.ID}&textOnly=1");
                }
                catch (WebException e)
                {
                    await Logging.ErrLog($"Plugin ID for {plugin.Name} invalid!\r\n\r\n{e}");
                    continue;
                }
                onlineVersion = onlineVersion.Replace("[a-zA-Z]", "").Split(" ")[0];
                var characters = new[]
                {
                    "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q",
                    "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H",
                    "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y",
                    "Z", "(", ")"
                };
                onlineVersion = characters.Aggregate(onlineVersion, (current, c) => current.Replace(c, string.Empty).Trim());
                var onlineVersionSplit = onlineVersion.Split(".");
                if (onlineVersionSplit.Length == 2) onlineVersion += ".0.0";
                if (onlineVersionSplit.Length == 3) onlineVersion += ".0";
                    
                try
                {
                    if (plugin.Version == onlineVersion) continue;
                    Console.WriteLine($"Updating Plugin {plugin.Name} from {plugin.Version} to {onlineVersion}");
                    if (string.IsNullOrEmpty(plugin.EAVersion) || plugin.EAVersion == "0")
                        await Logging.SendLog(0, 0, BasicEmbeds.Info($"Updating Plugin!\r\n{plugin.Name} from {plugin.Version} to {onlineVersion}", true), false);
                    else
                        await Logging.SendLog(0, 0, BasicEmbeds.Warning($"Updating Plugin!\r\n{plugin.Name} from {plugin.Version} to {onlineVersion}\r\nThis plugin has an EA version of {plugin.EAVersion} please double check it now!", true), false);
                    using IDbConnection cnn = new MySqlConnection(ConnStr);
                    await cnn.ExecuteAsync($"UPDATE Plugin SET Version = '{onlineVersion}' WHERE Name = '{plugin.Name}';");
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e);
                    Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Logging.ErrLog($"SQL Issue: {e}");
            }
        }
        await Task.Run(() => Program.Cache.UpdatePlugins(LoadPlugins()));
    }
}