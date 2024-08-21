using Dapper;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions.Messages;
using MySqlConnector;
using Npgsql;

namespace LSPDFRHelper.Functions;

public static class DbManager
{
    private static readonly string ConnStr = $"Host={Program.BotSettings.Env.DbServer};User ID={Program.BotSettings.Env.DbUser};Password={Program.BotSettings.Env.DbPass};Database={Program.BotSettings.Env.DbName};SearchPath={Program.BotSettings.Env.SchemaName}";
        
    // Case Functions
    public static List<AutoCase> GetCases()
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<AutoCase>("select * from cases").ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static AutoCase GetCase(string caseId)
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<AutoCase>($"select * from cases where caseid='{caseId}'").First();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static async void AddCase(AutoCase acase)
    {
        try
        {
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"insert into cases (caseid, ownerid, channelid, serverid, solved, tsrequested, requestid, createdate, expiredate) VALUES (@CaseId, {Convert.ToInt64(acase.OwnerId)}, {Convert.ToInt64(acase.ChannelId)}, {Convert.ToInt64(acase.ServerId)}, @Solved, @TsRequested, {Convert.ToInt64(acase.RequestId)}, @CreateDate, @ExpireDate)", acase);
            Program.Cache.UpdateCases(GetCases());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static async void EditCase(AutoCase acase)
    {
        try
        {
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"UPDATE cases SET ownerid = {Convert.ToInt64(acase.OwnerId)}, channelid = {Convert.ToInt64(acase.ChannelId)}, serverid = {Convert.ToInt64(acase.ServerId)}, solved = @Solved, tsrequested = @TsRequested, requestid = {Convert.ToInt64(acase.RequestId)}, createdate = @CreateDate, expiredate = @ExpireDate WHERE caseid = (@CaseId)", acase);
            Program.Cache.UpdateCases(GetCases());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    // Plugin Functions
    public static List<Plugin> GetPlugins()
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<Plugin>("select * from plugin").ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static Plugin GetPlugin(string pluginname)
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<Plugin>($"select * from plugin where name='{pluginname}'").First();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static List<Plugin> FindPlugins(string name = null, string dName = null, string id = null, State? state = null, PluginType? type = null, string description = null, bool? exactMatch = false)
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            List<string> conditions = new();
            var comparisonOperator = " = '";
            var endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }
            if (name != null) conditions.Add("name" + comparisonOperator + name + endOfComparison);
            if (dName != null) conditions.Add("dname" + comparisonOperator + dName + endOfComparison);
            if (id != null) conditions.Add("id" + comparisonOperator + id + endOfComparison);
            if (state != null) conditions.Add("state" + comparisonOperator + state + endOfComparison);
            if (type != null) conditions.Add("plugintype" + comparisonOperator + type + endOfComparison);
            if (description != null) conditions.Add("description" + comparisonOperator + description + endOfComparison);
            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            var conditionsString = string.Join(" and ", conditions);
            return cnn.Query<Plugin>($"select * from plugin where {conditionsString}").ToList();
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
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"insert into plugin (name, dname, version, eaversion, id, state, plugintype, link, description, authorid, announce) VALUES (@Name, @DName, @Version, @EaVersion, @Id, @State, @PluginType, @Link, @Description, {Convert.ToInt64(plugin.AuthorId)}, @Announce)", plugin);
            Program.Cache.UpdatePlugins(GetPlugins());
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
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"UPDATE plugin SET dname = @DName, version = @Version, eaversion = @EaVersion, id = @Id, state = @State, plugintype = @PluginType, link = @Link, description = @Description, authorid = {Convert.ToInt64(plugin.AuthorId)}, announce = @Announce WHERE Name = (@Name)", plugin);
            Program.Cache.UpdatePlugins(GetPlugins());
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
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from plugin where name = (@Name)", plugin);
            Program.Cache.UpdatePlugins(GetPlugins());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    // Error Functions
    public static List<Error> GetErrors()
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<Error>("select * from error").ToList();
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
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<Error>($"select * from error where id='{errorId}'").First();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static List<Error> FindErrors(string id, string pattern, string solution, string description, Level? level, bool? exactMatch = false)
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            List<string> conditions = new();
            var comparisonOperator = " = '";
            var endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }
            if (id != null) conditions.Add("id" + comparisonOperator + id + endOfComparison);
            if (pattern != null) conditions.Add("pattern" + comparisonOperator + pattern + endOfComparison);
            if (solution != null) conditions.Add("solution" + comparisonOperator + solution + endOfComparison);
            if (description != null) conditions.Add("description" + comparisonOperator + description + endOfComparison);
            conditions.Add("level" + comparisonOperator + level + endOfComparison);
                
            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            var conditionsString = string.Join(" and ", conditions);
            return cnn.Query<Error>($"select * from error where {conditionsString}", new DynamicParameters()).ToList();
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
        
    public static int AddError(Error error)
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            cnn.Execute("insert into error (pattern, solution, description, level, stringmatch) VALUES (@Pattern, @Solution, @Description, @Level, @StringMatch)", error);
            Program.Cache.UpdateErrors(GetErrors());
            return int.Parse(cnn.ExecuteScalar("SELECT id FROM error ORDER BY id DESC LIMIT 1;")!.ToString()!);
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static async void EditError(Error error)
    {
        try
        {
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE error SET pattern = @Pattern, solution = @Solution, description = @Description, level = @Level, stringmatch = @StringMatch WHERE Id = (@Id)", error);
            Program.Cache.UpdateErrors(GetErrors());
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
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from error where id = (@Id)", error);
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    // User Functions
    public static List<User> GetUsers()
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<User>("select * from users").ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static async void AddUser(User user)
    {
        try
        {
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"insert into users (id, username, boteditor, botadmin, blocked, logpath) VALUES ({Convert.ToInt64(user.Id)}, @Username, @BotEditor, @BotAdmin, @Blocked, @LogPath)", user);
            Program.Cache.UpdateUsers(GetUsers());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
        
    public static async void EditUser(User user)
    {
        try
        {
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"UPDATE users SET id = {Convert.ToInt64(user.Id)}, username = @Username, boteditor = @BotEditor, botadmin = @BotAdmin, blocked = @Blocked, logpath = @LogPath WHERE id = {Convert.ToInt64(user.Id)}", user);
            Program.Cache.UpdateUsers(GetUsers());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    // Misc Functions
    public static bool AutoHelperStatus(ulong serverid, bool? state = null)
    {
        if (state == null)
        {
            try
            {
                using var cnn = new NpgsqlConnection(ConnStr);
                return cnn.Query<bool>($"select ahenabled from serveroptions where serverid = '{Convert.ToInt64(serverid)}'").First();
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
            using var cnn = new NpgsqlConnection(ConnStr);
            cnn.Execute($"UPDATE serveroptions SET ahenabled = {state} WHERE serverid = '{Convert.ToInt64(serverid)}'");
            var output = !state;
            return output.Value;
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
        
    public static async void AddServer(Server server)
    {
        try
        {
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"insert into serveroptions (serverId, name, ownerid, enabled, blocked, ahenabled, autohelperchid, monitorchid, announcechid, managerroleid) VALUES ({Convert.ToInt64(server.ServerId)}, @Name, {Convert.ToInt64(server.OwnerId)}, @Enabled, @Blocked, @AhEnabled, {Convert.ToInt64(server.AutoHelperChId)}, {Convert.ToInt64(server.MonitorChId)}, {Convert.ToInt64(server.AnnounceChId)}, {Convert.ToInt64(server.ManagerRoleId)})", server);
            Program.Cache.UpdateServers();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
        
    public static async void EditServer(Server server)
    {
        try
        {
            await using var cnn = new NpgsqlConnection(ConnStr);
            await cnn.ExecuteAsync($"UPDATE serveroptions SET serverId = {Convert.ToInt64(server.ServerId)}, name = @Name, ownerid = {Convert.ToInt64(server.OwnerId)}, enabled = @Enabled, blocked = @Blocked, ahenabled = @AhEnabled, autohelperchid = {Convert.ToInt64(server.AutoHelperChId)}, monitorchid = {Convert.ToInt64(server.MonitorChId)}, announcechid = {Convert.ToInt64(server.AnnounceChId)}, managerroleid = {Convert.ToInt64(server.ManagerRoleId)} WHERE ServerId = {Convert.ToInt64(server.ServerId)}", server);
            Program.Cache.UpdateServers();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
        
    public static List<Server> GetServers()
    {
        try
        {
            using var cnn = new NpgsqlConnection(ConnStr);
            return cnn.Query<Server>("select * from serveroptions").ToList();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
}