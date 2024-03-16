using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using ULSS_Helper.Services;
using FuzzySharp;

namespace ULSS_Helper.Public.AutoHelper.Modules.Process_Files;
public class ImageProcess
{
    internal static async Task ProcessImage(DiscordAttachment attachment, MessageCreateEventArgs ctx)
    {
        try
        {
            DiscordMessageBuilder messageBuilder = new();
            var publicEmbed = BasicEmbeds.Public("## __ULSS AutoHelper__");

            var oskaruApiService = new OskaruApiService();
            var imageText = await oskaruApiService.GetImageText(attachment.Url, ctx.Message.Content);
            var imageTextSanitized = imageText.Replace("\n", " ").Replace("\r", " ").Trim();
            
            var logEmbedContent = new StringBuilder("**__Uploaded image was processed__**\r\n\r\n");
            logEmbedContent.Append($"Sender: <@{ctx.Message.Author.Id}>\r\n");
            logEmbedContent.Append($"Message: {ctx.Message.JumpLink}\r\n");
            logEmbedContent.Append($"Image: [{attachment.FileName}]({attachment.Url}) ({attachment.FileSize / 1000}KB)\r\n");
            if (string.IsNullOrEmpty(imageText))
            {
                logEmbedContent.Append($"No text recognized in uploaded image\r\n");
                var logNoTextEmbed = BasicEmbeds.Info(logEmbedContent.ToString());
                Logging.SendPubLog(logNoTextEmbed);
                return;
            }
            logEmbedContent.Append($"Recognized text: ```{imageText}```\r\n");
            
            var pimgErrors = Database.LoadErrors().Where(error => error.Level == "PIMG").ToList();
            var allScores = pimgErrors
                .Select(error => new
                    {
                        Error = error,
                        Score = Fuzz.PartialRatio(imageTextSanitized, error.Regex)
                    }
                ).ToList();
            
            var allMatches = allScores
                .Where(match => match.Score > 0)
                .OrderByDescending(match => match.Score)
                .ToList();
            
            var scoreOverThreshold = allMatches
                .Where(match => match.Score > 66) // Adjust the threshold as needed. Higher number => exact match. 
                .ToList();
            
            Error closestMatch = scoreOverThreshold 
                .Select(match => match.Error)
                .FirstOrDefault();

            if (closestMatch != null)
            {
                publicEmbed.AddField(
                    "\r\nCommon issue detected in uploaded image:", 
                    $"> {closestMatch.Regex.Replace("\n", "\n> ")}"
                );
                publicEmbed.AddField($"Suggested troubleshooting steps (ID {closestMatch.ID}):", $"> {closestMatch.Solution.Replace("\n", "\n> ")}\r\n");
            }
            
            if (scoreOverThreshold.Count > 0)
            {
                var matchedErrorIds = scoreOverThreshold.Select(match => $"{match.Error.ID} ({match.Score}%)").ToList();
                logEmbedContent.Append($"Matched with error IDs: {string.Join(", ", matchedErrorIds)}");
                messageBuilder.AddEmbed(publicEmbed);
                await ctx.Message.RespondAsync(messageBuilder);
            }
            else
            {
                logEmbedContent.Append($"Matched with error IDs: None");
            }
            
            var logEmbed = BasicEmbeds.Info(logEmbedContent.ToString());
            Logging.SendPubLog(logEmbed);
        }
        catch (Exception e)
        {
            Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
            throw;
        }
    }
}