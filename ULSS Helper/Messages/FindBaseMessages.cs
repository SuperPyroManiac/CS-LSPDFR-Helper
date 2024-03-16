using System.Text;
using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Messages;

internal class FindBaseMessages
{
    internal static int ChangesCount;
    
    internal static string GetModifiedPropertiesList(List<ModifiedProperty> properties)
    {
        var output = new StringBuilder();
        foreach (var prop in properties)
        {
            if (prop.OldValue == null) continue;
            if (prop.OldValue.Equals(prop.NewValue))
            {
                output.Append(prop.DefaultOutput);
            }
            else 
            {
                output.Append($"**{prop.Label}:**\r\n```diff\r\n");
                var diffText = GenerateDiff(prop.OldValue, prop.NewValue);
                output.Append(diffText + "```\r\n");
                ChangesCount++;
            }
        }
        return output.ToString();
    }

    private static string GenerateDiff(string oldText, string newText)
    {
        var oldLines = oldText.Split('\n');
        var newLines = newText.Split('\n');
        var maxLines = Math.Max(oldLines.Length, newLines.Length);

        var diffText = new StringBuilder();
        var countChangedLines = 0;

        for (var lineIdx = 0; lineIdx < maxLines; lineIdx++)
        {
            var oldLine = lineIdx < oldLines.Length ? oldLines[lineIdx] : null;
            var newLine = lineIdx < newLines.Length ? newLines[lineIdx] : null;
            
            if (oldLine == newLine || (oldLine != null && oldLine.Equals(newLine))) // line wasn't changed
            {
                oldLine = ReplaceDiffChars(oldLine);
                diffText.AppendLine($"{oldLine}");
            }
            else if (oldLine == null) // if oldLine doesn't exist, only show the added newLine (highlighted in green)
            {
                newLine = ReplaceDiffChars(newLine);
                diffText.AppendLine($"+ {newLine}");
                countChangedLines++;
            }
            else if (newLine == null) // if newLine doesn't exist, only show removed oldLine (highlighted in red)
            {
                oldLine = ReplaceDiffChars(oldLine);
                diffText.AppendLine($"- {oldLine}");
                countChangedLines++;
            }
            else // an existing line was modified, show the oldLine and newLine one after the other to allow an easy comparison
            {
                oldLine = ReplaceDiffChars(oldLine);
                newLine = ReplaceDiffChars(newLine);
                diffText.AppendLine($"- {oldLine}");
                diffText.AppendLine($"+ {newLine}");
                countChangedLines++;
            }
        }
        if (countChangedLines == maxLines) // if all lines have been changed, first show all removed lines, then show all new lines
        {
            var cleanedOld = ReplaceDiffChars(oldText, multiline: true);
            var cleanedNew = ReplaceDiffChars(newText, multiline: true);
            diffText = new StringBuilder();
            diffText.Append("- " + cleanedOld.Replace("\n", "\n- "));
            diffText.Append("\r\n+ " + cleanedNew.Replace("\n", "\n+ "));
        }
        return diffText.ToString();
    }

    /// <summary>
    /// Replaces any occurrences of "+ " or "- " at the start of lines in the input text. These characters would otherwise be interpreted as "added line" or "removed line" in the diff view.
    /// </summary>
    /// <param name="input">The input text that should be scanned and converted.</param>
    /// <param name="multiline">Whether the input text should be handled as a multiline text (line breaks) or not.</param>
    /// <returns>The changed input text where the characters are replaced.</returns>
    private static string ReplaceDiffChars(string input, bool multiline=false)
    {
        var option = multiline ? RegexOptions.Multiline : RegexOptions.None;
        var regexDash = new Regex("^- ", options: option);
        var output = multiline ? regexDash.Replace(input, " - ") : regexDash.Replace(input, " - ", count: 1);
        var regexPlus = new Regex("^+ ", options: option);
        output = multiline ? regexPlus.Replace(output, " + ") : regexDash.Replace(input, " - ", count: 1);
        return output;
    }
}