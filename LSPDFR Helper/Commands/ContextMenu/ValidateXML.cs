using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.LogTypes;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Processors.XML;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.ContextMenu;

public class ValidateXML
{
    [Command("Validate XML")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    public async Task ValidateXmlCmd(SlashCommandContext ctx, DiscordMessage targetMessage)
    {
        if (!await PermissionManager.RequireNotBlacklisted(ctx)) return;

        var msg = new DiscordInteractionResponseBuilder();
        msg.IsEphemeral = true;

        if ( targetMessage.Attachments.Count == 0 )
        {
            await ctx.RespondAsync(msg.AddEmbed(BasicEmbeds.Error("__No Files!__\r\n>>> No files were attached to this message!")));
            return;
        }

        var parsedXmls = new List<XmlLog>();
        var validCnt = 0;

        foreach ( var attach in targetMessage.Attachments )
        {
            if ( !attach.FileName!.EndsWith(".xml") && !attach.FileName!.EndsWith(".meta") ) continue;
            if ( validCnt >= 5 )
            {
                await ctx.RespondAsync(msg.AddEmbed(BasicEmbeds.Error("__Too Many XML Files!__\r\n>>> You may only validate up to 5 at a time! The first 5 will be validated, the rest will be skipped.")));
                break;
            }
            validCnt++;
            
            parsedXmls.Add(new XmlLog
            {
                FileName = attach.FileName,
                ParsedInfo = await XmlValidator.Run(attach.Url)
            });
        }
        
        if ( validCnt == 0 )
        {
            await ctx.RespondAsync(msg.AddEmbed(BasicEmbeds.Error("__No XML Files!__\r\n>>> No valid files were attached to this message.\r\n**Accepted types:**\r\n- .xml\r\n- .meta")));
            return;
        }

        var response = BasicEmbeds.Ts($"## __XML Validator__{BasicEmbeds.AddBlanks(35)}", new DiscordEmbedBuilder.EmbedFooter
        {
            IconUrl = ctx.User.AvatarUrl,
            Text = $"Sent by: {ctx.User.Username}"
        });
        foreach ( var log in parsedXmls )
        {
            response.AddField(log.FileName, $">>> ```xml\r\n{log.ParsedInfo}\r\n```");
        }

        await ctx.RespondAsync(msg.AddEmbed(BasicEmbeds.Success($"__All Files Validated!__{BasicEmbeds.AddBlanks(25)}\r\n>>> Results have been posted.")));
        await targetMessage.RespondAsync(response);
    }
}