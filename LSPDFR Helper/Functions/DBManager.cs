using System.Data;
using Dapper;
using LSPDFR_Helper.CustomTypes.SpecialTypes;
using LSPDFR_Helper.Functions.Messages;
using MySqlConnector;

namespace LSPDFR_Helper.Functions;

internal class DbManager
{
    private static readonly string ConnStr = $"Server={Program.BotSettings.Env.DbServer};User ID={Program.BotSettings.Env.DbUser};Password={Program.BotSettings.Env.DbPass};Database={Program.BotSettings.Env.DbName}";

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