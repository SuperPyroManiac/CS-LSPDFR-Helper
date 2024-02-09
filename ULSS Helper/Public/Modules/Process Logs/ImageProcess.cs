using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Tesseract;

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
            
            DiscordEmbedBuilder embed = new()
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
                        Console.WriteLine("Image downloaded successfully!");

                        var image = Pix.LoadFromMemory(imageData);
                        var page = engine.Process(image);

                        var text = page.GetText().Trim();
                        
                        embed.AddField("I detected the following text in the uploaded image:", $"```{text}```");
                        
                        foreach (var error in Database.LoadErrors().Where(error => error.Level == "AUTO"))
                        {
                            var errregex = new Regex(error.Regex);
                            var errmatch = errregex.Match(text);
                            if (errmatch.Success)
                                embed.AddField($"My suggested solution (ID: {error.ID}):",$"{error.Solution}");
                        }

                        messageBuilder.AddEmbed(embed);
                        await ctx.Message.RespondAsync(messageBuilder);
                    }
                    else
                    {
                        Console.WriteLine("Error downloading image: " + response.StatusCode);
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