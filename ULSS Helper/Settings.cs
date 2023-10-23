namespace ULSS_Helper;
using System.IO;

internal static class Settings
{
    internal static readonly string Token = GetToken();
    private static readonly string dbpath = Path.Combine(Directory.GetCurrentDirectory(), "ULSSDB.db");
    internal static string DbLocation = $"Data Source={dbpath};Version=3;";
    internal static string RphLogPath = Path.Combine(Directory.GetCurrentDirectory(), "RPHLogs", "RPHLog-1.log");
    private static string LogName = "RPHLog-1";
    private static int LogNumber;

    internal static string RPHVer = "1.106.1330.16514";
    internal static string LSPDFRVer = "0.4.8678.25591";
    internal static string GTAVer = "1.0.3028.0";

    private static string GetToken()
    {
        var tokenPath = Path.Combine(Directory.GetCurrentDirectory(), "TOKEN.txt");
        if (File.Exists(tokenPath)) return File.ReadAllText(tokenPath);
        File.Create(tokenPath);
        throw new FileNotFoundException("Token file could not be found. One has been created for you. Please add your token to the file.", "TOKEN.txt");
    }
    internal static ulong GetServerID()
    {
        var tokenPath = Path.Combine(Directory.GetCurrentDirectory(), "SERVERID.txt");
        if (File.Exists(tokenPath)) return Convert.ToUInt64(File.ReadAllText(tokenPath));
        Console.WriteLine("No SERVERID.txt found, using default value!");
        return 449706194140135444;
    }
    internal static ulong GetTSRole()
    {
        var tokenPath = Path.Combine(Directory.GetCurrentDirectory(), "TSROLE.txt");
        if (File.Exists(tokenPath)) return Convert.ToUInt64(File.ReadAllText(tokenPath));
        Console.WriteLine("No TSROLE.txt found, using default value!");
        return 517568233360982017;
    }
    
    internal static string LogNamer()
    {
        LogNumber++;
        LogName = $"RPHLog-{LogNumber}.log";
        RphLogPath = Path.Combine(Directory.GetCurrentDirectory(), "RPHLogs", LogName);
        return LogName;
    }

}