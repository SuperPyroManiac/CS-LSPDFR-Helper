using System.Data;
using System.Data.SQLite;
using System.Net;
using Dapper;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Database
{
    internal static List<Plugin> LoadPlugins()
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
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

    internal static Plugin GetPlugin(string pluginName)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
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
    
    internal static List<Plugin> FindPlugins(string name=null, string dName=null, string id=null, State? state=null, string description=null, bool? exactMatch=false)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            List<string> conditions = new List<string>();
            string comparisonOperator = " = '";
            string endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }

            if (name != null) conditions.Add("Name" + comparisonOperator + name + endOfComparison);
            if (dName != null) conditions.Add("DName" + comparisonOperator + dName + endOfComparison);
            if (id != null) conditions.Add("ID" + comparisonOperator + id + endOfComparison);
            if (state != null) conditions.Add("State" + comparisonOperator + state + endOfComparison);
            if (description != null) conditions.Add("Description" + comparisonOperator + description + endOfComparison);

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
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
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

    internal static Error GetError(string errorId)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
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

    internal static List<Error> FindErrors(string id, string regex, string solution, string description, Level? level, bool? exactMatch=false)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            List<string> conditions = new List<string>();
            string comparisonOperator = " = '";
            string endOfComparison = "'";
            if (!(exactMatch ?? true))  
            {
                comparisonOperator = " like '%";
                endOfComparison = "%'";
            }

            if (id != null) conditions.Add("ID" + comparisonOperator + id + endOfComparison);
            if (regex != null) conditions.Add("Regex" + comparisonOperator + regex + endOfComparison);
            if (solution != null) conditions.Add("Solution" + comparisonOperator + solution + endOfComparison);
            if (description != null) conditions.Add("Description" + comparisonOperator + description + endOfComparison);
            conditions.Add("Level" + comparisonOperator + level + endOfComparison);
            
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
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Open();
            cnn.Execute("insert into Plugin (Name, DName, Version, EAVersion, ID, State, Description, Link) VALUES (@Name, @DName, @Version, @EAVersion, @ID, @State, @Description, @Link)", plugin);
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
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Open();
            cnn.Execute("insert into Error (Regex, Solution, Description, Level) VALUES (@Regex, @Solution, @Description, @Level)", error);
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
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Execute("UPDATE Plugin SET (Name, DName, Version, EAVersion, ID, State, Description, Link) = (@Name, @DName, @Version, @EAVersion, @ID, @State, @Description, @Link) WHERE Name = (@Name)", plugin);
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
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Execute("UPDATE Error SET (Regex, Solution, Description, Level) = (@Regex, @Solution, @Description, @Level) WHERE ID = (@ID)", error);
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
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Execute("delete from Plugin where Name = (@Name)", plugin);
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
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Execute("delete from Error where ID = (@ID)", error);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static List<TS> LoadTs()
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
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
    
    internal static long AddTs(TS ts)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
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
    
    internal static void EditTs(TS ts)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Execute("UPDATE TS SET (ID, Username, View, Allow) = (@ID, @Username, @View, @Allow) WHERE ID = (@ID)", ts);
        }
        catch (SQLiteException e)
        {
            Console.WriteLine(e);
            Messages.Logging.ErrLog($"SQL Issue: {e}");
            throw;
        }
    }
    
    internal static void DeleteTs(TS ts)
    {
        try
        {
            using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
            cnn.Execute("delete from TS where ID = (@ID)", ts);
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
#pragma warning disable SYSLIB0014
	    var webClient = new WebClient();
	    var plugins = LoadPlugins();
        foreach (var plugin in plugins)
        {
            try
            {
                if (plugin.State == "LSPDFR" && plugin.ID != null)
                {
                    var onlineVersion = webClient.DownloadString($"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={plugin.ID}&textOnly=1");
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
                    if (onlineVersionSplit.Length == 2) onlineVersion += ".0.0";
                    if (onlineVersionSplit.Length == 3) onlineVersion += ".0";
                    
                    try
                    {
                        Console.WriteLine($"Updating Plugin {plugin.Name} from {plugin.Version} to {onlineVersion}");
                        
                        using IDbConnection cnn = new SQLiteConnection(Program.Settings.DbLocation);
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