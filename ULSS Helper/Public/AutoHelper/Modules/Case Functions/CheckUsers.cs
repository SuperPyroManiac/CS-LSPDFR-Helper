namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class CheckUsers
{
    internal static async Task Validate()
    {
        var cases = Program.Cache.GetCasess().Where(x => x.Solved == 0).ToList();
        var users = Program.GetGuild().Members.Values.ToList();

        foreach (var ac in cases.Where(ac => !users.Any(x => x.Id.ToString().Equals(ac.OwnerID))))
            await CloseCase.Close(ac);
    }
    
    internal static async Task CloseCases(string uid)
    {
        var cases = Program.Cache.GetCasess().Where(x => x.Solved == 0).ToList();
        
        foreach (var ac in cases.Where(ac => ac.OwnerID == uid))
            await CloseCase.Close(ac);
    }
}