using System.Xml.Serialization;
using LSPDFRHelper.CustomTypes.Enums;

namespace LSPDFRHelper.CustomTypes.MainTypes;

public class Plugin
{
    public string Name { get; set; }
    public string DName { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string EaVersion { get; set; }
    public int Id { get; set; }
    public string Link { get; set; }
    public ulong AuthorId { get; set; }
    public bool Announce { get; set; }
    public PluginType PluginType { get; set; }
    public State State { get; set; }

    public string LinkedName()
    {
        if ( string.IsNullOrEmpty(Link) ) 
            return $"[{DName}](https://www.google.com/search?q=lspdfr+{DName.Replace(" ", "+")})";
        return $"[{DName}]({Link})";
    }

    public Plugin Clone()
    {
        using var stream = new MemoryStream();
        var xml = new XmlSerializer(typeof(Plugin));
        xml.Serialize(stream, this);
        stream.Position = 0;
        return (Plugin)xml.Deserialize(stream);
    }
}