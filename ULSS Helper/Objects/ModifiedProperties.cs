namespace ULSS_Helper.Objects;

public class ModifiedProperty
{
    public string Label { get; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string DefaultOutput { get; }

    public ModifiedProperty(string label, string oldValue, string newValue, string defaultOutput)
    {
        Label = label;
        OldValue = oldValue;
        NewValue = newValue;
        DefaultOutput = defaultOutput;
    }
}