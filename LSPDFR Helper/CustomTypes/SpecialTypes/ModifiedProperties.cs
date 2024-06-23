namespace LSPDFR_Helper.CustomTypes.SpecialTypes;

public class ModifiedProperty(string label, string oldValue, string newValue, string defaultOutput)
{
    public string Label { get; } = label;
    public string OldValue { get; set; } = oldValue;
    public string NewValue { get; set; } = newValue;
    public string DefaultOutput { get; } = defaultOutput;
}