
using System.Reflection;

namespace ULSS_Helper.Modules.Messages;

internal class DbCmdMessages
{
    static internal int ChangesCount = 0;
    internal const string SHOULD_BE_SKIPPED = "should_be_skipped";
    
    internal static string GetModifiedPropertiesList(object oldObj, object newObj, List<string> labels, List<string> defaultPropLines)
    {
        string text = "";
        for (int i=0; i < newObj.GetType().GetProperties().Length; i++)
        {
            PropertyInfo oldObjProp = oldObj.GetType().GetProperties()[i];
            PropertyInfo newObjProp = newObj.GetType().GetProperties()[i];
            Type? type = Nullable.GetUnderlyingType(newObjProp.PropertyType) ?? newObjProp.PropertyType;

            string? oldObjPropValue = oldObjProp.GetValue(oldObj)?.ToString();
            string? newObjPropValue = newObjProp.GetValue(newObj)?.ToString();
            if (oldObjPropValue == null || newObjPropValue == null || defaultPropLines[i].Equals(SHOULD_BE_SKIPPED))
                continue;
            
            if (oldObjPropValue.Equals(newObjPropValue))
            {
                text += defaultPropLines[i];
            }
            else 
            {
                text += $"**{labels[i]}:**\r\n```diff\r\n";
                string diffText = "";

                var oldLines = oldObjPropValue.Split("\n");
                var newLines = newObjPropValue.Split("\n");
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
                    if (!string.IsNullOrEmpty(oldLine) && !string.IsNullOrEmpty(newLine))
                    {
                        diffText += $"- {oldLine}\n";
                        diffText += $"+ {newLine}\n";
                        countChangedLines++;
                        continue;
                    }
                    if (string.IsNullOrEmpty(newLine))
                    {
                        diffText += $"- {oldLine}\n";
                        countChangedLines++;
                        continue;
                    }
                    if (string.IsNullOrEmpty(oldLine))
                    {
                        diffText += $"+ {newLine}\n";
                        countChangedLines++;
                        continue;
                    }
                }
                if (countChangedLines == maxLines)
                {
                    diffText = "- " + oldObjPropValue.Replace("\n", $"\n- ");
                    diffText += "\r\n+ " + newObjPropValue.Replace("\n", $"\n+ ");
                }
                text += diffText + "```\r\n";
                ChangesCount++;
            }
        }
        return text;
    }
}