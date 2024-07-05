using Dapper;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.CustomTypes.SpecialTypes;
using LSPDFRHelper.Functions.Messages;
using MySqlConnector;

namespace LSPDFRHelper.Functions;

public static class DbManager
{
    private static readonly string ConnStr = $"Server={Program.BotSettings.Env.DbServer};User ID={Program.BotSettings.Env.DbUser};Password={Program.BotSettings.Env.DbPass};Database={Program.BotSettings.Env.DbName}";
    
    //Case Functions
    public static List<AutoCase> GetCases()
    {
        try
        {
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<AutoCase>("select * from Cases").ToList();
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
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<AutoCase>($"select * from Cases where CaseID='{caseId}'").First();
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Cases (CaseId, OwnerID, ChannelID, Solved, TsRequested, RequestID, CreateDate, ExpireDate) VALUES (@CaseId, @OwnerID, @ChannelID, @Solved, @TsRequested, @RequestID, @CreateDate, @ExpireDate)", acase);
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Cases SET OwnerID = @OwnerID, ChannelID = @ChannelID, Solved = @Solved, TsRequested = @TsRequested, RequestID = @RequestID, CreateDate = @CreateDate, ExpireDate = @ExpireDate WHERE CaseId = (@CaseId)", acase);
            Program.Cache.UpdateCases(GetCases());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    //Plugin Functions
    public static List<Plugin> GetPlugins()
    {
        try
        {
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<Plugin>("select * from Plugin").ToList();
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
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<Plugin>($"select * from Plugin where Name='{pluginname}'").First();
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
            using var cnn = new MySqlConnection(ConnStr);
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
            if (id != null) conditions.Add("Id" + comparisonOperator + id + endOfComparison);
            if (state != null) conditions.Add("State" + comparisonOperator + state + endOfComparison);
            if (type != null) conditions.Add("PluginType" + comparisonOperator + type + endOfComparison);
            if (description != null) conditions.Add("Description" + comparisonOperator + description + endOfComparison);

            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            var conditionsString = string.Join(" and ", conditions);
            return cnn.Query<Plugin>($"select * from Plugin where {conditionsString}").ToList();
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Plugin (Name, DName, Version, EaVersion, Id, State, PluginType, Link, Description, AuthorId, Announce) VALUES (@Name, @DName, @Version, @EaVersion, @Id, @State, @PluginType, @Link, @Description, @AuthorId, @Announce)", plugin);
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Plugin SET DName = @DName, Version = @Version, EaVersion = @EaVersion, Id = @Id, State = @State, PluginType = @PluginType, Link = @Link, Description = @Description, AuthorId = @AuthorId, Announce = @Announce WHERE Name = (@Name)", plugin);
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from Plugin where Name = (@Name)", plugin);
            Program.Cache.UpdatePlugins(GetPlugins());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    //Error Functions
    public static List<Error> GetErrors()
    {
        try
        {
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<Error>("select * from Error").ToList();
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
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<Error>($"select * from Error where Id='{errorId}'").First();
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
            using var cnn = new MySqlConnection(ConnStr);
            List<string> conditions = [];
            var comparisonOperator = " = '";
            var endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }

            if (id != null) conditions.Add("Id" + comparisonOperator + id + endOfComparison);
            if (pattern != null) conditions.Add("Pattern" + comparisonOperator + pattern + endOfComparison);
            if (solution != null) conditions.Add("Solution" + comparisonOperator + solution + endOfComparison);
            if (description != null) conditions.Add("Description" + comparisonOperator + description + endOfComparison);
            conditions.Add("Level" + comparisonOperator + level + endOfComparison);
            
            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            var conditionsString = string.Join(" and ", conditions);
            return cnn.Query<Error>($"select * from Error where {conditionsString}", new DynamicParameters()).ToList();
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
            using var cnn = new MySqlConnection(ConnStr);
            cnn.Execute("insert into Error (Pattern, Solution, Description, Level, StringMatch) VALUES (@Pattern, @Solution, @Description, @Level, @StringMatch)", error);
            Program.Cache.UpdateErrors(GetErrors());
            return int.Parse(cnn.ExecuteScalar("SELECT LAST_INSERT_ID();")!.ToString()!);
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Error SET Pattern = @Pattern, Solution = @Solution, Description = @Description, Level = @Level, StringMatch = @StringMatch WHERE Id = (@Id)", error);
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("delete from Error where Id = (@Id)", error);
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    //User Functions
    public static List<User> GetUsers()
    {
        try
        {
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<User>("select * from Users").ToList();
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Users (Id, Username, BotEditor, BotAdmin, Blocked, LogPath) VALUES (@Id, @Username, @BotEditor, @BotAdmin, @Blocked, @LogPath)", user);
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
            await using var cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Users SET Id = @Id, Username = @Username, BotEditor = @BotEditor, BotAdmin = @BotAdmin, Blocked = @Blocked, LogPath = @LogPath WHERE Id = @Id", user);
            Program.Cache.UpdateUsers(GetUsers());
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    //Msc Functions
    public static bool AutoHelperStatus(bool? state = null)
    {
        if (state == null)
        {
            try
            {
                using var cnn = new MySqlConnection(ConnStr);
                return cnn.Query<bool>($"select AhStatus from ServerOptions where Id = '{Program.BotSettings.Env.BotId}'").First();
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
            using var cnn = new MySqlConnection(ConnStr);
            cnn.Execute($"UPDATE ServerOptions SET AhStatus = {state} WHERE Id = '{Program.BotSettings.Env.BotId}'");
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
    
    public static GlobalSettings GetGlobalSettings(string botid)
    {
        try
        {
            using var cnn = new MySqlConnection(ConnStr);
            return cnn.Query<GlobalSettings>($"select * from ServerOptions where Id = '{botid}'").First();
        }
        catch ( MySqlException e )
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
}