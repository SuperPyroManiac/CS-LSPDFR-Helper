using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ULSS_Helper.Services;

record AnalyseImageLinkRequest
{
    public string downloadLink { get; set; }
    public string caption { get; set; }
}

record AnalyseImageLinkResponse
{
    public string ocrResult { get; set; }
}

public class OskaruApiService
{
    private HttpClient _httpClient;
    
    public OskaruApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(Program.Settings.Env.OskaruApiBaseUrl);
        _httpClient.DefaultRequestHeaders.ConnectionClose = true;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", Program.Settings.Env.OskaruApiKey);
    }

    public async Task<string> GetImageText(string imageUrl, string caption = null)
    {
        Console.WriteLine("Got to oska");
        var requestBody = new AnalyseImageLinkRequest
        {
            downloadLink = imageUrl,
            caption = caption
        };
        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string jsonContent = JsonSerializer.Serialize(requestBody, serializeOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        content.Headers.ContentLength = jsonContent.Length;

        try
        {
            // Send the POST request
            HttpResponseMessage response = await _httpClient.PostAsync("v1/gta-helper/analyse-image/link", content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                AnalyseImageLinkResponse responseObj = JsonSerializer.Deserialize<AnalyseImageLinkResponse>(responseBody);
                return responseObj.ocrResult;
            }

            Console.WriteLine("Request failed with status code: " + response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Request error: " + ex.Message);
        }
        return "";
    }
}