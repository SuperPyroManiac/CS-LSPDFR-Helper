using System.Data;
using System.Data.SQLite;
using System.Net;
using Dapper;

namespace ULSS_Helper.Modules;

internal class DatabaseManager
{
    internal static List<Plugin> LoadPlugins()
    {
        using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
        var output = cnn.Query<Plugin>("select * from Plugin", new DynamicParameters());
        return output.ToList();
    }

    internal static void AddPlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute("insert into Plugin (Name, DName, Version, EAVersion, ID, State, Link) VALUES (@Name, @DName, @Version, @EAVersion, @ID, @State, @Link)", plugin);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    internal static void EditPlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute($"UPDATE Plugin SET (Name, DName, Version, EAVersion, ID, State, Link) = (@Name, @DName, @Version, @EAVersion, @ID, @State, @Link) WHERE Name = (@Name)", plugin);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    internal static void AddError(Error error)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute("insert into Error (Regex, Solution, Level) VALUES (@Regex, @Solution, @Level)", error);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    internal static void DeletePlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute($"delete from Plugin where Name = (@Name)", plugin);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    internal static void UpdatePluginVersions()
    {
        var webClient = new WebClient();
        var plugins = LoadPlugins();
        var nm = 1;
        foreach (var plugin in plugins)
        {
            Thread.Sleep(250);
            try
            {
                if (plugin.State == "LSPDFR" && plugin.ID != null)
                {
                    var plugincheck = plugin.Name + ", Version=" + plugin.Version;
                    var url =
                        "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=" +
                        plugin.ID + "&textOnly=1";
                    var onlineVersion = webClient.DownloadString(url);
                    onlineVersion = onlineVersion.Replace("[a-zA-Z]", "").Split(" ")[0];
                    var characters = new[]
                    {
                        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q",
                        "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H",
                        "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y",
                        "Z", "(", ")"
                    };
                    foreach (var c in characters) onlineVersion = onlineVersion.Replace(c, string.Empty).Trim();
                    var onlineVersionSplit = onlineVersion.Split(".");
                    if (onlineVersionSplit.Length == 2) onlineVersion = onlineVersion + ".0.0";
                    if (onlineVersionSplit.Length == 3) onlineVersion = onlineVersion + ".0";
                    
                    try
                    {
                        Console.WriteLine($"Updating Plugin {plugin.Name} from {plugin.Version} to {onlineVersion}");
                        nm++;
                        
                        using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
                        cnn.Execute($"UPDATE Plugin SET Version = '{onlineVersion}' WHERE Name = '{plugin.Name}';");
                    }
                    catch (SQLiteException e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}