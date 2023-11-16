using System.Data;
using System.Data.SQLite;
using System.Net;
using Dapper;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Database //TODO: Make strings safe
{
    internal static List<Plugin> LoadPlugins()
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            var output = cnn.Query<Plugin>("select * from Plugin");
            return output.ToList();
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    internal static Plugin? GetPlugin(string pluginName)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            var output = cnn.Query<Plugin>($"select * from Plugin where Name='{pluginName}'");
            output = output.ToList();
            if (output.Count() == 1) 
            {
                return output.First();
            }
            if (output.Count() > 1)
            {
                throw new Exception($"GetPlugin unexpectedly returned more than one result for the plugin name '{pluginName}'!");
            }
            return null;
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static List<Plugin> FindPlugins(string? Name=null, string? DName=null, string? ID=null, State? State=null, bool? exactMatch=false)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            List<string> conditions = new List<string>();
            string comparisonOperator = " = '";
            string endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }

            if (Name != null) conditions.Add("Name" + comparisonOperator + Name + endOfComparison);
            if (DName != null) conditions.Add("DName" + comparisonOperator + DName + endOfComparison);
            if (ID != null) conditions.Add("ID" + comparisonOperator + ID + endOfComparison);
            if (State != null) conditions.Add("State" + comparisonOperator + State.ToString() + endOfComparison);

            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            string conditionsString = string.Join(" and ", conditions);
            var output = cnn.Query<Plugin>($"select * from Plugin where {conditionsString}");

            return output.ToList();
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
        catch (InvalidDataException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    internal static List<Error> LoadErrors()
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            var output = cnn.Query<Error>("select * from Error");
            return output.ToList();
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    internal static Error? GetError(string errorId)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            var output = cnn.Query<Error>($"select * from Error where ID='{errorId}'");
            output = output.ToList();
            if (output.Count() == 1) 
            {
                return output.First();
            }
            if (output.Count() > 1)
            {
                throw new Exception($"GetError unexpectedly returned more than one result for the error ID '{errorId}'!");
            }
            return null;
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    internal static List<Error> FindErrors(string? ID, string? Regex, string? Solution, Level? Level, bool? exactMatch=false)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            List<string> conditions = new List<string>();
            string comparisonOperator = " = '";
            string endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }

            if (ID != null) conditions.Add("ID" + comparisonOperator + ID.ToString() + endOfComparison);
            if (Regex != null) conditions.Add("Regex" + comparisonOperator + Regex + endOfComparison);
            if (Solution != null) conditions.Add("Solution" + comparisonOperator + Solution + endOfComparison);
            if (Level != null) conditions.Add("Level" + comparisonOperator + Level.ToString() + endOfComparison);
            
            if (conditions.Count == 0) throw new InvalidDataException("At least one of the input parameters has to have a non-null value!");
            string conditionsString = string.Join(" and ", conditions);
            var output = cnn.Query<Error>($"select * from Error where {conditionsString}", new DynamicParameters());

            return output.ToList();
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
        catch (InvalidDataException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }

    internal static long AddPlugin(Plugin plugin)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Open();
            cnn.Execute("insert into Plugin (Name, DName, Version, EAVersion, ID, State, Link) VALUES (@Name, @DName, @Version, @EAVersion, @ID, @State, @Link)", plugin);
            long id = ((SQLiteConnection) cnn).LastInsertRowId;
            cnn.Close();
            return id;
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static long AddError(Error error)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Open();
            cnn.Execute("insert into Error (Regex, Solution, Level) VALUES (@Regex, @Solution, @Level)", error);
            long id = ((SQLiteConnection) cnn).LastInsertRowId;
            cnn.Close();
            return id;    
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
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
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static void EditError(Error error)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute($"UPDATE Error SET (Regex, Solution, Level) = (@Regex, @Solution, @Level) WHERE ID = (@ID)", error);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
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
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static void DeleteError(Error error)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute($"delete from Error where ID = (@ID)", error);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static List<TS> LoadTS()
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            var output = cnn.Query<TS>("select * from TS");
            return output.ToList();
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static long AddTS(TS ts)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Open();
            cnn.Execute("insert into TS (ID, Username, View, Allow) VALUES (@ID, @Username, @View, @Allow)", ts);
            long id = ((SQLiteConnection) cnn).LastInsertRowId;
            cnn.Close();
            return id;    
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static void EditTS(TS ts)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute($"UPDATE Error SET (Username, View, Allow) = (@Username, @View, @Allow) WHERE ID = (@ID)", ts);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static void DeleteTS(TS ts)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
            cnn.Execute($"delete from TS where ID = (@ID)", ts);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static void UpdatePluginVersions()
    {
        var webClient = new WebClient();
        var plugins = LoadPlugins();
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
                        
                        using IDbConnection cnn = new SQLiteConnection(Settings.DbLocation);
                        cnn.Execute($"UPDATE Plugin SET Version = '{onlineVersion}' WHERE Name = '{plugin.Name}';");
                    }
                    catch (SQLiteException e)
                    {
                        Console.WriteLine(e);
                        Messages.Logging.ErrLog($"SQL Issue: {e}");
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Messages.Logging.ErrLog($"SQL Issue: {e}");
                throw;
            }
        }
    }
}