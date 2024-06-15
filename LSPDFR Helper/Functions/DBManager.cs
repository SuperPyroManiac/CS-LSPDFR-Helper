using System.Data;
using Dapper;
using LSPDFR_Helper.CustomTypes.SpecialTypes;
using MySqlConnector;

namespace LSPDFR_Helper.Functions;

internal class DbManager
{
    private static readonly string ConnStr = $"Server={Program.Settings.Env.DbServer};User ID={Program.Settings.Env.DbUser};Password={Program.Settings.Env.DbPass};Database={Program.Settings.Env.DbName}";

    internal static Dictionary<string, string> GetGlobalSettings()
    {
        using IDbConnection cnn = new MySqlConnection(ConnStr);
        var dict = cnn.Query("select * from GlobalValues").ToDictionary(
            row => (string)row.Name,
            row => (string)row.Value);
        return dict;
    }
}