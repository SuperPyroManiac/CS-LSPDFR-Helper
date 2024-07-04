using System.Xml;
using System.Xml.Linq;

namespace LSPDFRHelper.Functions.Processors.XML;

public class XmlValidator
{
    public static async Task<string> Run(string path)
    {
        var xmlFile = await new HttpClient().GetStringAsync(path);
        
        try {
            var contacts = XElement.Parse(xmlFile);
            return "XML is valid!";
        }
        catch (XmlException e)
        {
            return e.Message;
        }
    }
}