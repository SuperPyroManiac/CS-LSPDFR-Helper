namespace ULSS_Helper;
using System.IO;
using ULSS_Helper.Objects;

internal static class Settings
{
    internal static readonly string Token = GetToken();
    internal static readonly string dbpath = Path.Combine(Directory.GetCurrentDirectory(), "ULSSDB.db");
    internal static string DbLocation = $"Data Source={dbpath};Version=3;";
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
        return 449706194140135444;
    }
    internal static ulong GetTSRole()
    {
        var tokenPath = Path.Combine(Directory.GetCurrentDirectory(), "TSROLE.txt");
        if (File.Exists(tokenPath)) return Convert.ToUInt64(File.ReadAllText(tokenPath));
        return 517568233360982017;
    }
    
    internal static string GenerateNewFilePath(FileType fileType)
    {
        string fileName;
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd_HH-mm-ss-fff");

        switch(fileType)
        {
            case FileType.RPH_LOG:
                fileName = $"RagePluginHook_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder("RPHLogs"), fileName);

            case FileType.ELS_LOG:
                fileName = $"ELS_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder("ELSLogs"), fileName);

            case FileType.ASI_LOG:
                fileName = $"asiloader_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder("ASILogs"), fileName);

            case FileType.SHVDN_LOG:
                fileName = $"ScriptHookVDotNet_{formattedDateTime}.log";
                return Path.Combine(GetOrCreateFolder( "SHVDNLogs"), fileName);

            case FileType.DB_BACKUP:
                fileName = $"ULSSDB_{formattedDateTime}.db";
                return Path.Combine(GetOrCreateFolder( "Backups"), fileName);

            default:
                throw new ArgumentException("Invalid FileType!");
        }
    }

    private static string GetOrCreateFolder(string folder)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), folder);
        if (!Path.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }
}
