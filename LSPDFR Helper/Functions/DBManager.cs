using Dapper;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.CustomTypes.SpecialTypes;
using LSPDFR_Helper.Functions.Messages;
using MySqlConnector;

namespace LSPDFR_Helper.Functions;

public static class DbManager
{
    private static readonly string ConnStr = $"Server={Program.BotSettings.Env.DbServer};User ID={Program.BotSettings.Env.DbUser};Password={Program.BotSettings.Env.DbPass};Database={Program.BotSettings.Env.DbName}";

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
    
    public static int AddError(Error error)
    {
        try
        {
            using var cnn = new MySqlConnection(ConnStr);
            cnn.Execute("insert into Error (Pattern, Solution, Description, Level, StringMatch) VALUES (@Pattern, @Solution, @Description, @Level, @StringMatch)", error);
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
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    //Msc Functions
    public static GlobalSettings GetGlobalSettings()
    {
        try
        {
            using var cnn = new MySqlConnection(ConnStr);
            var dict = cnn.Query("select * from GlobalValues").ToDictionary(
                row => ( string )row.Name,
                row => ( string )row.Value);
            return new GlobalSettings
            {
                GTAVer = dict["GTAVer"],
                RPHVer = dict["RPHVer"],
                LSPDFRVer = dict["LSPDFRVer"],
                SHVVer = dict["SHVVer"],
                AHStatus = dict["AHStatus"].Equals("1"),
                ServerId = ulong.Parse(dict["ServerId"]),
                TsRoleId = ulong.Parse(dict["TsRoleId"]),
                TsIconUrl = dict["TsIconUrl"],
                AutoHelperChId = ulong.Parse(dict["AutoHelperChId"]),
                SupportChId = ulong.Parse(dict["SupportChId"]),
                MonitorChId = ulong.Parse(dict["MonitorChId"]),
                BotLogChId = ulong.Parse(dict["BotLogChId"]),
                PublicLogChId = ulong.Parse(dict["PublicLogChId"]),
                ReportChId = ulong.Parse(dict["ReportChId"]),
                StaffContactChId = ulong.Parse(dict["StaffContactChId"])
            };
        }
        catch ( MySqlException e )
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
}