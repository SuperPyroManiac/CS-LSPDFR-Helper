// using System.ComponentModel;
// using DSharpPlus.Commands;
// using DSharpPlus.Commands.Processors.SlashCommands;
// using DSharpPlus.Entities;
// using LSPDFRHelper.CustomTypes.CacheTypes;
// using LSPDFRHelper.EventManagers;
// using LSPDFRHelper.Functions.Messages;
// using LSPDFRHelper.Functions.Verifications;
//
// namespace LSPDFRHelper.Commands;
//
// public class EditUser
// {
//     [Command("edituser")]
//     [Description("Edits a user!")]
//     public async Task EditUserCmd(SlashCommandContext ctx, [Description("User to edit!")] DiscordUser user)
//     {
//         if (!await PermissionManager.RequireBotAdmin(ctx)) return;
//         var bd = new DiscordInteractionResponseBuilder();
//         bd.IsEphemeral = true;
//         if (Program.Cache.GetUsers().All(x => x.Id.ToString() != user.Id.ToString()))
//         {
//             await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
//                 bd.AddEmbed(BasicEmbeds.Error("__User is not in the DB!__")));
//             return;
//         }
//         
//         var dUser = Program.Cache.GetUser(user.Id);
//         var tempus = await ctx.Guild!.GetMemberAsync(dUser.Id);
//         dUser!.Username = tempus.Username;
//         
//         DiscordInteractionResponseBuilder modal = new();
//         modal.WithCustomId(CustomIds.SelectUserValueToEdit);
//         modal.WithTitle($"Editing {dUser.Username}!");
//         modal.AddComponents(new DiscordTextInputComponent(
//             label: "Editor:", 
//             customId: "userEditor", 
//             required: false,
//             style: DiscordTextInputStyle.Short, 
//             value: dUser.BotEditor.ToString()
//         ));
//         modal.AddComponents(new DiscordTextInputComponent(
//             label: "BotAdmin:", 
//             customId: "userBotAdmin", 
//             required: false,
//             style: DiscordTextInputStyle.Short, 
//             value: dUser.BotAdmin.ToString()
//         ));
//         modal.AddComponents(new DiscordTextInputComponent(
//             label: "Blacklisted:", 
//             customId: "userBlacklist", 
//             required: false, 
//             style: DiscordTextInputStyle.Short, 
//             value: dUser.Blocked.ToString()
//         ));
//         
//         Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new InteractionCache(ctx.Interaction, dUser));
//         await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
//     }
// }