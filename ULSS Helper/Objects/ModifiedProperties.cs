namespace ULSS_Helper.Objects;

public class ModifiedProperty
{
    public string Label { get; }
    public string OldValue { get; }
    public string NewValue { get; }
    public string DefaultOutput { get; }

    public ModifiedProperty(string label, string oldValue, string newValue, string defaultOutput)
    {
        Label = label;
        OldValue = oldValue;
        NewValue = newValue;
        DefaultOutput = defaultOutput;
    }
}