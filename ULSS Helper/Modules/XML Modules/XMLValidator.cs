using System.Xml;
using System.Xml.Linq;

namespace ULSS_Helper.Modules.XML_Modules;

public class XMLValidator
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