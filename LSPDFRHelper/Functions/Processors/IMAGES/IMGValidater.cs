using LSPDFRHelper.Functions.Messages;
using Tesseract;

namespace LSPDFRHelper.Functions.Processors.IMAGES
{
    public static class IMGValidater
    {
        public static async Task<string> Run(string imgLink)
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(imgLink);
                response.EnsureSuccessStatusCode();
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                return new TesseractEngine(@"./tessdata", "eng", EngineMode.Default).Process(Pix.LoadFromMemory(imageBytes)).GetText();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Logging.ErrLog($"{e}");
                return null;
            }
        }
    }
}