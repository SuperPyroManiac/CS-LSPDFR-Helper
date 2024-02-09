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
                        
                        if (String.IsNullOrEmpty(text))
                        {
                            var logNoTextEmbed = BasicEmbeds.Info("No text recognized in uploaded image");
                            logNoTextEmbed.AddField("Image:", $"[Link](<{attachment.Url}>)");
                            Logging.SendLog(ctx.Message.Channel.Id, ctx.Message.Author.Id, logNoTextEmbed);
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
                                
                                // response to public user
                                publicEmbed.AddField(
                                    "\r\nCommon issue detected in uploaded image:", 
                                    $"> {errmatch.Value}"
                                );
                                publicEmbed.AddField($"Suggested troubleshooting steps (ID {error.ID}):", $"> {error.Solution.Replace("\n", "\n> ")}\r\n");
                            }
                        }

                        
                        // ts-bot-log
                        var logEmbedContentBuilder = new StringBuilder("**__Uploaded image was processed__**\r\n\r\n");
                        logEmbedContentBuilder.Append($"Sender: <@{ctx.Message.Author.Id}>\r\n");
                        logEmbedContentBuilder.Append($"Channel: <#{ctx.Message.Channel.Id}>\r\n");
                        logEmbedContentBuilder.Append($"Image: [{attachment.FileName}]({attachment.Url}) ({attachment.FileSize / 1000}KB)\r\n");
                        logEmbedContentBuilder.Append($"Recognized text: ```{text}```\r\n");
                        if (matchedErrors.Count > 0)
                        {
                            var matchedErrorIds = matchedErrors.Select(matchedError => matchedError.ID).ToList();
                            logEmbedContentBuilder.Append($"Matched with error IDs: {string.Join(", ", matchedErrorIds)}");
                            
                        }
                        else
                        {
                            publicEmbed.AddField(
                                $"No common issues found in the text within the uploaded image. Try the following:\n", 
                                "- if you think your screenshot contains some kind of text or error message that corresponds to a common issue that I know about...\n" +
                                " - try creating a higher quality screenshot (no phone pictures of your computer screen), use the PRINT-SCREEN key or Windows Key + Shift + S.\n" +
                                " - try cropping the screenshot before uploading it here so it only shows the error message and no other unrelated text on your screen.\n" +
                                "- if the above applies to you but you already tried the suggested steps, feel free to request help from a human.\n" +
                                "" +
                                "\r\n"
                            );
                            logEmbedContentBuilder.Append($"Matched with error IDs: None");
                        }
                        
                        var logEmbed = BasicEmbeds.Info(logEmbedContentBuilder.ToString());
                        Logging.SendPubLog(logEmbed);
                        
                        messageBuilder.AddEmbed(publicEmbed);
                        await ctx.Message.RespondAsync(messageBuilder);
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
            ULSS_Helper.Messages.Logging.ErrLog(e.ToString());//TODO: Blacklist
            Console.WriteLine(e);
            throw;
        }
    }
}