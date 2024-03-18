using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using DiscordUser = DSharpPlus.Entities.DiscordUser;

namespace ULSS_Helper.Commands;

public class EditUser : ApplicationCommandModule
{
    [SlashCommand("EditUser", "Edits a user!")]
    [RequireBotAdmin]
    public async Task EditUserCmd(
        InteractionContext ctx, 
        [Option("ID", "User ID to edit!")] DiscordUser userId)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadUsers().All(x => x.UID.ToString() != userId.Id.ToString()))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("User is not in the DB!")));
            return;
        }

        var dUser = Database.LoadUsers().FirstOrDefault(x => x.UID.ToString() == userId.Id.ToString());
        var tmpusr = await ctx.Guild.GetMemberAsync(ulong.Parse(dUser.UID));
        dUser!.Username = tmpusr.Username;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId(ModalSubmit.EditUser);
        modal.WithTitle($"Editing {dUser.Username}!");
        modal.AddComponents(new TextInputComponent(
            label: "Editor:", 
            customId: "userEditor", 
            required: false,
            style: TextInputStyle.Short, 
            value: dUser.BotEditor.ToString()
        ));
        modal.AddComponents(new TextInputComponent(
            label: "BotAdmin:", 
            customId: "userBotAdmin", 
            required: false,
            style: TextInputStyle.Short, 
            value: dUser.BotAdmin.ToString()
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Blacklisted:", 
            customId: "userBlacklist", 
            required: false, 
            style: TextInputStyle.Short, 
            value: dUser.Blocked.ToString()
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Bully:", 
            customId: "userBully", 
            required: false, 
            style: TextInputStyle.Short, 
            value: dUser.Bully.ToString()
        ));
        
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new UserActionCache(ctx.Interaction, dUser));
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}