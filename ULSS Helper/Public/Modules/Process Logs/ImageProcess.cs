using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Tesseract;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.Modules.Process_Logs;
public class ImageProcess
{
    internal static async Task ProcessImage(DiscordAttachment attachment, MessageCreateEventArgs ctx)
    {
        try
        {
            DiscordMessageBuilder messageBuilder = new();
            
            // 1. download the English OCR training data from here: https://github.com/tesseract-ocr/tessdata/blob/main/eng.traineddata
            // 2. create a folder called "tessdata" (on the root level where ULSSDB.db and environment-config.json files are located, too)
            // 3. move the downloaded "eng.traineddata" file into the created "tessdata" folder
            var engine = new TesseractEngine(@".\tessdata", "eng");
            
            DiscordEmbedBuilder publicEmbed = new()
            {
                Description = $"## __ULSS AutoHelper__",
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Disclaimer: This is an experimental feature and may not be as accurate as my log analysis."
                }
            };
            
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(attachment.Url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageData = await response.Content.ReadAsByteArrayAsync();
                        var image = Pix.LoadFromMemory(imageData);
                        var page = engine.Process(image);

                        var text = page.GetText().Trim();
                        var textNoLineBreaks = text.Replace("\n", " ").Replace("\r", " ");
                        
                        var logEmbedContent = new StringBuilder("**__Uploaded image was processed__**\r\n\r\n");
                        logEmbedContent.Append($"Sender: <@{ctx.Message.Author.Id}>\r\n");
                        logEmbedContent.Append($"Channel: <#{ctx.Message.Channel.Id}>\r\n");
                        logEmbedContent.Append($"Image: [{attachment.FileName}]({attachment.Url}) ({attachment.FileSize / 1000}KB)\r\n");
                        if (String.IsNullOrEmpty(text))
                            logEmbedContent.Append($"No text recognized in uploaded image\r\n");
                        else
                            logEmbedContent.Append($"Recognized text: ```{text}```\r\n");
                        
                        if (String.IsNullOrEmpty(text))
                        {
                            var logNoTextEmbed = BasicEmbeds.Info(logEmbedContent.ToString());
                            Logging.SendPubLog(logNoTextEmbed);
                            return;
                        }

                        var matchedErrors = new List<Error>();
                        foreach (var error in Database.LoadErrors().Where(error => error.Level == "AUTO"))
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
                    else
                    {
                        throw new Exception("Error downloading image: " + response.StatusCode);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logging.ErrLog(e.ToString());//TODO: Blacklist
            Console.WriteLine(e);
            throw;
        }
    }
}