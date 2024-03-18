namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class CheckUsers
{
    internal static async Task Validate()
    {
        var cases = Database.LoadCases().Where(x => x.Solved == 0).ToList();
        var server = await Program.Client.GetGuildAsync(Program.Settings.Env.ServerId);
        var users = server.Members;

        foreach (var ac in cases.Where(ac => !users.ContainsKey(ulong.Parse(ac.OwnerID))))
            await CloseCase.Close(ac);
    }
    
    internal static async Task CloseCases(string uid)
    {
        var cases = Database.LoadCases().Where(x => x.Solved == 0).ToList();
        
        foreach (var ac in cases.Where(ac => ac.OwnerID == uid))
            await CloseCase.Close(ac);
    }
}