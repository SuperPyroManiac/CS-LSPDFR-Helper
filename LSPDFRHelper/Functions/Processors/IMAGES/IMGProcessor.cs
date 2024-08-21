using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FuzzySharp;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Processors.IMAGES;

public class IMGProcessor
{
        public static async Task ProcessImage(DiscordAttachment attachment, MessageCreatedEventArgs ctx)
    {
        try
        {
            DiscordMessageBuilder messageBuilder = new();
            var publicEmbed = BasicEmbeds.Public("## __LSPDFR AutoHelper__");

            var imageText = await IMGValidater.Run(attachment.Url);
            var imageTextSanitized = imageText.Replace("\n", " ").Replace("\r", " ").Trim();
            
            var logEmbedContent = new StringBuilder("**__Uploaded image was processed__**\r\n\r\n>>> ");
            logEmbedContent.Append($"**Sender:** <@{ctx.Message.Author!.Id}>\r\n");
            logEmbedContent.Append($"**Message:** {ctx.Message.JumpLink}\r\n");
            logEmbedContent.Append($"**Server:** {ctx.Guild.Name}\r\n");
            logEmbedContent.Append($"**Image:** [{attachment.FileName}]({attachment.Url}) ({attachment.FileSize / 1000}KB)\r\n");
            if (string.IsNullOrEmpty(imageText))
            {
                logEmbedContent.Append("*No text recognized in uploaded image*\r\n");
                var logNoTextEmbed = BasicEmbeds.Info(logEmbedContent.ToString());
                await Logging.SendPubLog(logNoTextEmbed);
                return;
            }
            logEmbedContent.Append($"**Recognized text:** ```{imageText}```\r\n");
            
            var pimgErrors = Program.Cache.GetErrors().Where(error => error.Level == Level.PIMG).ToList();
            var allScores = pimgErrors
                .Select(error => new
                    {
                        Error = error,
                        Score = Fuzz.PartialRatio(imageTextSanitized, error.Pattern)
                    }
                ).ToList();
            var allMatches = allScores
                .Where(match => match.Score > 0)
                .OrderByDescending(match => match.Score)
                .ToList();
            
            var scoreOverThreshold = allMatches
                .Where(match => match.Score > 50) // Adjust the threshold as needed. Higher number => exact match. 
                .ToList();
            
            var closestMatch = scoreOverThreshold 
                .Select(match => match.Error)
                .FirstOrDefault();

            if (closestMatch != null)
            {
                publicEmbed.AddField(
                    "\r\nCommon issue detected in uploaded image:", 
                    $"> {closestMatch.Pattern.Replace("\n", "\n> ")}"
                );
                publicEmbed.AddField($"Suggested troubleshooting steps (ID {closestMatch.Id}):", $"> {closestMatch.Solution.Replace("\n", "\n> ")}\r\n");
            }
            
            if (scoreOverThreshold.Count > 0)
            {
                var matchedErrorIds = scoreOverThreshold.Select(match => $"{match.Error.Id} ({match.Score}%)").ToList();
                logEmbedContent.Append($"Matched with error IDs: {string.Join(", ", matchedErrorIds)}");
                messageBuilder.AddEmbed(publicEmbed);
                await ctx.Message.RespondAsync(messageBuilder);
            }
            else
            {
                logEmbedContent.Append("Matched with error IDs: None");
            }
            
            var logEmbed = BasicEmbeds.Info(logEmbedContent.ToString(), true);
            await Logging.SendPubLog(logEmbed);
        }
        catch (Exception e)
        {
            await Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
            throw;
        }
    }
}