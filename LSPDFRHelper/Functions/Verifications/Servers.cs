using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Verifications;

public static class Servers
{
    public static async Task<int> AddMissing()
    {
        try
        {
            var dsc =
                "__LSPDFR Helper Added!__" +
                "\r\nThis bot is able to read a multitude of different log types! It can provide solutions to many common problems, and has info on every plugin added to LSPDFR!" +
                "\r\n" +
                "\r\nCreated by SuperPyroManiac with the help of Hammer and Hendrik from ULSS, RPH, and DG." +
                "\r\nFor more information see https://dsc.PyrosFun.com" +
                "\r\n" +
                "\r\nYou can delete this message or click the buttons below for more info!" +
                "\r\n*Suggestions or support? [Contact the developer!](https://dsc.PyrosFun.com)*";
            var emb = BasicEmbeds.Info(dsc);
            emb.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://i.imgur.com/jxODw4N.png" };
            var msg = new DiscordMessageBuilder().AddEmbed(emb);
            msg.AddComponents(
            [
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, CustomIds.SelectSetupInfo, "Setup Info", false, new DiscordComponentEmoji("🛠️")),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.SelectCommandInfo, "Command Info", false, new DiscordComponentEmoji("📋"))
            ]);
            
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
                        if ( serv.Value.SystemChannelId != null )
                        {
                            var ch = await Program.Client.GetChannelAsync(serv.Value.SystemChannelId.Value);
                            await ch.SendMessageAsync(msg);
                        }

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
                // if ( Program.Cache.GetUser(serv.Value.OwnerId) != null )
                //     if ( Program.Cache.GetUser(serv.Value.OwnerId).Blocked ) srv.Blocked = true;
                DbManager.AddServer(srv);
                if ( serv.Value.SystemChannelId != null )
                {
                    var ch = await Program.Client.GetChannelAsync(serv.Value.SystemChannelId.Value);
                    await ch.SendMessageAsync(msg);
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