using System.Data;
using Dapper;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.CustomTypes.SpecialTypes;
using LSPDFR_Helper.Functions.Messages;
using MySqlConnector;

namespace LSPDFR_Helper.Functions;

internal class DbManager
{
    private static readonly string ConnStr = $"Server={Program.BotSettings.Env.DbServer};User ID={Program.BotSettings.Env.DbUser};Password={Program.BotSettings.Env.DbPass};Database={Program.BotSettings.Env.DbName};Allow User Variables=True";

    //Error Functions
    internal static List<Error> GetErrors()
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
            var output = cnn.Query<Error>($"select * from Error where Id='{errorId}'");
            cnn.Close();
            return output.First();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    internal static long AddError(Error error)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            cnn.Open();
            cnn.Execute("insert into Error (Pattern, Solution, Description, Level, StringMatch) VALUES (@Pattern, @Solution, @Description, @Level, @StringMatch)", error);
            var id = cnn.ExecuteScalar("SELECT LAST_INSERT_ID();")!;
            cnn.Close();
            return long.Parse(id.ToString()!);
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            Logging.ErrLog($"SQL Issue: {e}").GetAwaiter();
            throw;
        }
    }
    
    internal static async void EditError(Error error)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Error SET Pattern = @Pattern, Solution = @Solution, Description = @Description, Level = @Level, StringMatch = @StringMatch WHERE Id = (@Id)", error);
            cnn.Close();
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
            await cnn.ExecuteAsync("delete from Error where Id = (@Id)", error);
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    //User Functions
    internal static List<User> GetUsers()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            var output = cnn.Query<User>("select * from Users");
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
    
    
    internal static async void AddUser(User user)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("insert into Users (Id, Username, BotEditor, BotAdmin, Blocked, LogPath) VALUES (@Id, @Username, @BotEditor, @BotAdmin, @Blocked, @LogPath)", user);
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static async void EditUser(User user)
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
            await cnn.ExecuteAsync("UPDATE Users SET Id = @Id, Username = @Username, BotEditor = @BotEditor, BotAdmin = @BotAdmin, Blocked = @Blocked, LogPath = @LogPath WHERE Id = @Id", user);
            cnn.Close();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    //Msc Functions
    internal static GlobalSettings GetGlobalSettings()
    {
        try
        {
            using IDbConnection cnn = new MySqlConnection(ConnStr);
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