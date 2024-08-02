using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Verifications;

public static class Servers
{
    public static async Task<int> AddMissing()
    {
        try
        {
            var cnt = 0;
            foreach (var serv in Program.Client.Guilds)
            {
                var dbserv = DbManager.GetServers().FirstOrDefault(x => x.ServerId == serv.Key);
                if ( dbserv != null )
                {
                    if ( !dbserv.Enabled )
                    {
                        dbserv.Enabled = true;
                        DbManager.EditServer(dbserv);
                        cnt++;
                    }

                    dbserv.Name = serv.Value.Name;
                    dbserv.OwnerId = serv.Value.OwnerId;
                    DbManager.EditServer(dbserv);
                    continue;
                }
                cnt++;
                var srv = new Server()
                {
                    ServerId = serv.Value.Id,
                    Name = serv.Value.Name,
                    OwnerId = serv.Value.OwnerId,
                    Enabled = true,
                    Blocked = false,
                    AhEnabled = true,
                    AutoHelperChId = 0,
                    MonitorChId = 0,
                    AnnounceChId = 0,
                    ManagerRoleId = 0
                };
                if ( Program.Cache.GetUser(serv.Value.OwnerId) != null )
                    if ( Program.Cache.GetUser(serv.Value.OwnerId).Blocked ) srv.Blocked = true;
                

                DbManager.AddServer(srv);
            }
            return cnt;
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
            return 0;
        }
    }

    public static async Task<int> RemoveMissing()
    {
        try
        {
            var cnt = 0;
            foreach ( var serv in DbManager.GetServers() )
            {
                try
                { _ = Program.Client.Guilds[serv.ServerId]; }
                catch ( Exception )
                {
                    if (!serv.Enabled) continue;
                    cnt++;
                    serv.Enabled = false;
                    DbManager.EditServer(serv);
                }
            }

            return cnt;
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
            return 0;
        }
    }

    public static async Task Validate()
    {
        try
        {
            foreach ( var serv in Program.Client.Guilds )
            {
                var server = Program.Cache.GetServer(serv.Key);
                if (server == null) continue;
                
                // if ( Program.Cache.GetUser(serv.Value.OwnerId) != null )
                //     if ( Program.Cache.GetUser(serv.Value.OwnerId).Blocked ) server.Blocked = true;
                // DbManager.EditServer(server);
            }
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
}