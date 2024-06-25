using System.Xml.Linq;

namespace LSPDFR_Helper.Functions.Processors.XML;

public class XmlValidator
{
    public static async Task<string> Run(string path)
    {
        var xmlFile = await new HttpClient().GetStringAsync(path);
        
        try {
            var contacts = XElement.Parse(xmlFile);
            return "XML is valid!";
        }
        catch (System.Xml.XmlException e)
        {
            return e.Message;
        }
    }
}