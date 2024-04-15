namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class CheckCases
{
    internal static async Task Validate()
    {
        var parentCh = await Program.Client.GetChannelAsync(Program.Settings.Env.AutoHelperChannelId);
        foreach (var th in parentCh.Threads)
        {
            if (th.ThreadMetadata.IsArchived) continue;
            foreach (var ac in Program.Cache.GetCasess())
            {
                if (!ac.ChannelID.Equals(th.Id.ToString())) continue;
                if (ac.Solved == 1 || ac.Timer == 0) await CloseCase.Close(ac);
            }
        }
    }
}