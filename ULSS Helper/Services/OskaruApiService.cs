using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ULSS_Helper.Services;

record AnalyseImageLinkRequest
{
    public String downloadLink { get; set; }
    public String caption { get; set; }
}

record AnalyseImageLinkResponse
{
    public String ocrResult { get; set; }
}

public class OskaruApiService
{
    private HttpClient _httpClient;
    
    public OskaruApiService()
    {
        this._httpClient = new HttpClient();
        this._httpClient.BaseAddress = new Uri(Program.Settings.Env.OskaruApiBaseUrl);
        this._httpClient.DefaultRequestHeaders.ConnectionClose = true;
        this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", Program.Settings.Env.OskaruApiKey);
    }

    public async Task<String> GetImageText(String imageUrl, String? caption = null)
    {
        Console.WriteLine("Got to oska");
        var requestBody = new AnalyseImageLinkRequest()
        {
            downloadLink = imageUrl,
            caption = caption
        };
        Console.WriteLine("o2");
        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        Console.WriteLine("o3");
        string jsonContent = JsonSerializer.Serialize(requestBody, serializeOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        content.Headers.ContentLength = jsonContent.Length;
        Console.WriteLine("o4");

        try
        {
            // Send the POST request
            HttpResponseMessage response = await this._httpClient.PostAsync("v1/gta-helper/analyse-image/link", content);
            Console.WriteLine("o5");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("o6");
                string responseBody = await response.Content.ReadAsStringAsync();
                AnalyseImageLinkResponse responseObj = JsonSerializer.Deserialize<AnalyseImageLinkResponse>(responseBody);
                Console.WriteLine("o7");
                return responseObj.ocrResult;
            }
            else
            {
                Console.WriteLine("Request failed with status code: " + response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Request error: " + ex.Message);
        }
        return "";
    }
}