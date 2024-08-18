
using System.Text;
using System.Text.RegularExpressions;

namespace PartyFinderPresets.Utils;

public static class Helpers
{
    public static string GapsBeforeCapitals(string x, bool replaceAnd = false){
        var str = Regex.Replace(x, "([A-Z])([a-z]*)", " $1$2");
        if(replaceAnd)
            str = Regex.Replace(str, "(and) ", "&"); // just for V&C Dungeon Finder
        return str;
    }

    public static string ByteArrayToString(byte[] bytes) {
        var i = 0;
        var stringBuilder = new StringBuilder("{");
        foreach(var b in bytes) {
            i++;
            stringBuilder.Append($"{b}, ");
        }
        stringBuilder.Append("} :" + i.ToString());
        return stringBuilder.ToString();
    }
}
