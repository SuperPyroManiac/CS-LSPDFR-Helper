using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Verifications;

public static class Servers
{
    public static Task<int> AddMissing()
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
                continue;
            }
            cnt++;
            DbManager.AddServer(new Server
            {
                ServerId = serv.Value.Id,
                Enabled = true,
                Blocked = false,
                AhEnabled = false,
                AutoHelperChId = 0,
                MonitorChId = 0,
                AnnounceChId = 0,
                ManagerRoleId = 0
            });
        }
        return Task.FromResult(cnt);
    }

    public static Task<int> RemoveMissing()
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

        return Task.FromResult(cnt);
    }

    public static async Task Validate()
    {
        foreach ( var serv in Program.Client.Guilds )
        {
            var server = Program.Cache.GetServer(serv.Key);
            if (server == null) continue;

            if ( server.Blocked )
            {
                //var owner = await serv.Value.GetGuildOwnerAsync();
                var owner = serv.Value.Owner;
                await Logging.ReportPubLog(
                    BasicEmbeds.Error($"__Left Blocked Server__\r\n>>> " + 
                                      $"**Name:** {serv.Value.Name}\r\n" +
                                      $"**ID:** {serv.Value.Id}\r\n" +
                                      $"**Owner:** {owner.Id} ({owner.Username})"));
                var ch = await serv.Value.GetPublicUpdatesChannelAsync();
                if ( ch != null )
                    await ch.SendMessageAsync(
                        BasicEmbeds.Error("__Server Is Blacklisted!__\r\n>>> If you think this is an error, you may contact us at https://dsc.PyrosFun.com"));
                await serv.Value.LeaveAsync();
            }
        }
    }
}