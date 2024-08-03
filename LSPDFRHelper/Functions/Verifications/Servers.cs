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
            var dsc =
                "__LSPDFR Helper Added!__" +
                "\r\n*Here is everything you need to know!*" +
                "\r\n" +
                "\r\n__**Initial Setup**__" +
                "\r\n> -# If you do not care to use the AutoHelper or have a support role, then you are done! You now have `Validate Log` in your context menu!" +
                "\r\n> - Choose a manager role you want to have access to the support commands." +
                "\r\n> - Choose a channel for the AutoHelper. Recommend creating an empty where people cannot type. The AutoHelper creates private threads in this channel that are used for the individual cases." +
                "\r\n> - Choose a channel for the AutoHelper Monitor. This shows all open cases as well as is where messages are posted when the request help button is used. Recommend setting this to a new channel where people cannot type." +
                "\r\n> - Run `/setup` and assign the channel id's and role id if applicable." +
                "\r\n" +
                "\r\n__**Usage**__" +
                "\r\n> Right click context menu apps:" +
                "\r\n> - `Validate Log`: This will process the selected logs. *(Public)*" +
                "\r\n> - `Validate XML`: This will parse XML and META files. *(Public)*" +
                "\r\n> Console commands:" +
                "\r\n> - `/setup`: Change bot settings. *(Server Admins)*" +
                "\r\n> - `/ToggleAH`: Enables or disables the AutoHelper. *(Manager Role)*" +
                "\r\n> - `/FindCases <User>`: Finds the last 25 AutoHelper cases from a user. *(Manager Role)*" +
                "\r\n> - `/CloseCase <Case/All>`: Closes the specified case. Can put `all` to close all cases. *(Manager Role)*" +
                "\r\n> - `/CheckPlugin <Plugin>`: View information on any plugin in our DB. *(Public)*" +
                "\r\n> - `/JoinCase <Case>`: Join an open AutoHelper case. *(Public)*" +
                "\r\n> **You can adjust these by setting up the integration permissions in your server settings!**" +
                "\r\n*Suggestions or support? [Contact the developer!](https://dsc.PyrosFun.com)*";
            var emb = BasicEmbeds.Info(dsc);
            emb.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://i.imgur.com/jxODw4N.png" };
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
                            await ch.SendMessageAsync(emb);
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
                    await ch.SendMessageAsync(emb);
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