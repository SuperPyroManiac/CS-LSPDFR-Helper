using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Tesseract;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using ULSS_Helper.Services;

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
            var text = await oskaruApiService.GetImageText(attachment.Url, ctx.Message.Content);

            var textNoLineBreaks = text.Replace("\n", " ").Replace("\r", " ");
            
            var logEmbedContent = new StringBuilder("**__Uploaded image was processed__**\r\n\r\n");
            logEmbedContent.Append($"Sender: <@{ctx.Message.Author.Id}>\r\n");
            logEmbedContent.Append($"Channel: <#{ctx.Message.Channel.Id}>\r\n");
            logEmbedContent.Append($"Image: [{attachment.FileName}]({attachment.Url}) ({attachment.FileSize / 1000}KB)\r\n");
            if (string.IsNullOrEmpty(text))
                logEmbedContent.Append($"No text recognized in uploaded image\r\n");
            else
                logEmbedContent.Append($"Recognized text: ```{text}```\r\n");
            
            if (string.IsNullOrEmpty(text))
            {
                var logNoTextEmbed = BasicEmbeds.Info(logEmbedContent.ToString());
                Logging.SendPubLog(logNoTextEmbed);
                return;
            }
            // Match against all errors. May need to use it's own error type for this.
            var matchedErrors = new List<Error>();
            foreach (var error in Database.LoadErrors().Where(x => x.Level == "PIMG"))
            {
                var errregex = new Regex(error.Regex);
                var errmatch = errregex.Match(textNoLineBreaks);
                if (errmatch.Success)
                {
                    matchedErrors.Add(error);
                    
                    publicEmbed.AddField(
                        "\r\nCommon issue detected in uploaded image:", 
                        $"> {errmatch.Value}"
                    );
                    publicEmbed.AddField($"Suggested troubleshooting steps (ID {error.ID}):", $"> {error.Solution.Replace("\n", "\n> ")}\r\n");
                }
            }
            
            if (matchedErrors.Count > 0)
            {
                var matchedErrorIds = matchedErrors.Select(matchedError => matchedError.ID).ToList();
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
            Logging.ErrLog(e.ToString());//TODO: Blacklist
            Console.WriteLine(e);
            throw;
        }
    }
    
    // requires Tesseract to be installed on the local machine
    private static async Task<string> getTextFromImageLocally(DiscordAttachment attachment)
    {
        var engine = new TesseractEngine(Path.Combine(Directory.GetCurrentDirectory(), "tessdata"), "eng");
        using (HttpClient client = new HttpClient())
        { 
            using (HttpResponseMessage response = await client.GetAsync(attachment.Url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var imageData = await response.Content.ReadAsByteArrayAsync();
                    var image = Pix.LoadFromMemory(imageData);
                    var processedImage = engine.Process(image);

                    return processedImage.GetText().Trim();
                }
                throw new Exception("Error downloading image: " + response.StatusCode);
            }
        }
    }
}