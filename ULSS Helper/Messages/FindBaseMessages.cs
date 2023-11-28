using System.Text;
using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Messages;

internal class FindBaseMessages
{
    internal static int ChangesCount;
    
    internal static string GetModifiedPropertiesList(List<ModifiedProperty> properties)
    {
        StringBuilder output = new StringBuilder();
        foreach (ModifiedProperty prop in properties)
        {
            if (prop.OldValue.Equals(prop.NewValue))
            {
                output.Append(prop.DefaultOutput);
            }
            else 
            {
                output.Append($"**{prop.Label}:**\r\n```diff\r\n");
                string diffText = GenerateDiff(prop.OldValue, prop.NewValue);
                output.Append(diffText + "```\r\n");
                ChangesCount++;
            }
        }
        return output.ToString();
    }

    private static string GenerateDiff(string oldText, string newText)
    {
        string[] oldLines = oldText.Split('\n');
        string[] newLines = newText.Split('\n');
        int maxLines = Math.Max(oldLines.Length, newLines.Length);

        StringBuilder diffText = new StringBuilder();
        int countChangedLines = 0;

        for (int lineIdx = 0; lineIdx < maxLines; lineIdx++)
        {
            string oldLine = lineIdx < oldLines.Length ? oldLines[lineIdx] : null;
            string newLine = lineIdx < newLines.Length ? newLines[lineIdx] : null;
            
            if (oldLine == newLine || (oldLine != null && oldLine.Equals(newLine)))
            {
                oldLine = ReplaceDiffChars(oldLine);
                diffText.AppendLine($"{oldLine}");
            }
            else if (oldLine == null)
            {
                newLine = ReplaceDiffChars(newLine);
                diffText.AppendLine($"+ {newLine}");
                countChangedLines++;
            }
            else if (newLine == null)
            {
                oldLine = ReplaceDiffChars(oldLine);
                diffText.AppendLine($"- {oldLine}");
                countChangedLines++;
            }
            else
            {
                oldLine = ReplaceDiffChars(oldLine);
                newLine = ReplaceDiffChars(newLine);
                diffText.AppendLine($"- {oldLine}");
                diffText.AppendLine($"+ {newLine}");
                countChangedLines++;
            }
        }
        if (countChangedLines == maxLines)
        {
            string cleanedOld = ReplaceDiffChars(oldText, multiline: true);
            string cleanedNew = ReplaceDiffChars(newText, multiline: true);
            diffText = new StringBuilder();
            diffText.Append("- " + cleanedOld.Replace("\n", "\n- "));
            diffText.Append("\r\n+ " + cleanedNew.Replace("\n", "\n+ "));
        }
        return diffText.ToString();
    }

    private static string ReplaceDiffChars(string input, bool multiline=false)
    {
        RegexOptions option = multiline ? RegexOptions.Multiline : RegexOptions.None;
        Regex regexDash = new Regex("^- ", options: option);
        string output = multiline ? regexDash.Replace(input, " - ") : regexDash.Replace(input, " - ", count: 1);
        Regex regexPlus = new Regex("^+ ", options: option);
        output = multiline ? regexPlus.Replace(output, " + ") : regexDash.Replace(input, " - ", count: 1);
        return output;
    }
}