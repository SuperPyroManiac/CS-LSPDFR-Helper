namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

public class CheckUsers
{
    public static async Task Validate()
    {
        var cases = Program.Cache.GetCasess().Where(x => x.Solved == 0).ToList();
        var users = Program.GetGuild().Members.Values.ToList();

        foreach (var ac in cases.Where(ac => !users.Any(x => x.Id.ToString().Equals(ac.OwnerID))))
            await CloseCase.Close(ac);
    }
    
    public static async Task CloseCases(string uid)
    {
        var cases = Program.Cache.GetCasess().Where(x => x.Solved == 0).ToList();
        
        foreach (var ac in cases.Where(ac => ac.OwnerID == uid))
            await CloseCase.Close(ac);
    }
}