using LSPDFRHelper.Functions.Messages;
using Tesseract;

namespace LSPDFRHelper.Functions.Processors.IMAGES;

public static class IMGValidater
{
    private static readonly TesseractEngine Engine = new("./tessdata", "eng", EngineMode.Default);
    public static async Task<string> Run(string imgLink)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync(imgLink);
            response.EnsureSuccessStatusCode();
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            using var page = Engine.Process(Pix.LoadFromMemory(imageBytes));
            return page.GetText();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog($"{e}");
            return null;
        }
    }
}