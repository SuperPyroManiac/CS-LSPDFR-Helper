﻿namespace ULSS_Helper;
using System.IO;

internal static class Settings
{
    internal static readonly string Token = GetToken();
    internal static readonly string dbpath = Path.Combine(Directory.GetCurrentDirectory(), "ULSSDB.db");
    internal static string DbLocation = $"Data Source={dbpath};Version=3;";
    internal static string RphLogPath = Path.Combine(Directory.GetCurrentDirectory(), "RPHLogs", "RPHLog-1.log");
    internal static string ElsLogPath = Path.Combine(Directory.GetCurrentDirectory(), "ELSLogs", "ELSLog-1.log");
    internal static string AsiLogPath = Path.Combine(Directory.GetCurrentDirectory(), "ASILogs", "ASILog-1.log");
    internal static string DbBackupPath = Path.Combine(Directory.GetCurrentDirectory(), "Backups", "DB-1.db");
    private static string LogName = "ZehFirstLog";
    private static int RPHLogNumber;
    private static int ELSLogNumber;
    private static int ASILogNumber;
    private static int DbNameNumber;

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
    
    internal static string RphLogNamer()
    {
        RPHLogNumber++;
        LogName = $"RPHLog-{RPHLogNumber}.log";
        RphLogPath = Path.Combine(Directory.GetCurrentDirectory(), "RPHLogs", LogName);
        return LogName;
    }
    internal static string ElsLogNamer()
    {
        ELSLogNumber++;
        LogName = $"ELSLog-{ELSLogNumber}.log";
        ElsLogPath = Path.Combine(Directory.GetCurrentDirectory(), "ELSLogs", LogName);
        return LogName;
    }
    internal static string AsiLogNamer()
    {
        ASILogNumber++;
        LogName = $"ASILog-{ASILogNumber}.log";
        AsiLogPath = Path.Combine(Directory.GetCurrentDirectory(), "ASILogs", LogName);
        return LogName;
    }
    
    internal static string DbBackupNamer()
    {
        DbNameNumber++;
        LogName = $"DB-{DbNameNumber}.db";
        DbBackupPath = Path.Combine(Directory.GetCurrentDirectory(), "Backups", LogName);
        return DbBackupPath;
    }

}