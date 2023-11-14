using ULSS_Helper.Objects;

namespace ULSS_Helper.Messages;

internal class FindBaseMessages
{
    static internal int ChangesCount = 0;
    internal const string SHOULD_BE_SKIPPED = "should_be_skipped";
    
    internal static string GetModifiedPropertiesList(List<ModifiedProperty> properties)
    {
        string text = "";
        foreach (ModifiedProperty prop in properties)
        {
            if (prop.OldValue == null || prop.NewValue == null)
                continue;
            
            if (prop.OldValue.Equals(prop.NewValue))
            {
                text += prop.DefaultOutput;
            }
            else 
            {
                text += $"**{prop.Label}:**\r\n```diff\r\n";
                string diffText = "";

                var oldLines = prop.OldValue.Split("\n");
                var newLines = prop.NewValue.Split("\n");
                int maxLines = Math.Max(oldLines.Length, newLines.Length);
                int countChangedLines = 0;
                for (int lineIdx=0; lineIdx < maxLines; lineIdx++)
                {
                    string? oldLine = lineIdx <= oldLines.Length-1 ? oldLines[lineIdx] : null;
                    string? newLine = lineIdx <= newLines.Length-1 ? newLines[lineIdx] : null;

                    if (oldLine != null && oldLine.Equals(newLine))
                    {
                        diffText += $"{oldLine}\n";
                        continue;
                    }
                    if (oldLine != null && newLine != null)
                    {
                        diffText += $"- {oldLine}\n";
                        diffText += $"+ {newLine}\n";
                        countChangedLines++;
                        continue;
                    }
                    if (newLine != null)
                    {
                        diffText += $"- {oldLine}\n";
                        countChangedLines++;
                        continue;
                    }
                    if (oldLine != null)
                    {
                        diffText += $"+ {newLine}\n";
                        countChangedLines++;
                        continue;
                    }
                }
                if (countChangedLines == maxLines)
                {
                    diffText = "- " + prop.OldValue.Replace("\n", $"\n- ");
                    diffText += "\r\n+ " + prop.NewValue.Replace("\n", $"\n+ ");
                }
                text += diffText + "```\r\n";
                ChangesCount++;
            }
        }
        return text;
    }
}