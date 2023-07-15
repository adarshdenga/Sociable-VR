using System.Text;


public static class Extensions {
    public static string RSplit(this string str, char ch) {
        if(string.IsNullOrEmpty(str))
            return str;
        var idx = str.LastIndexOf(ch);
        return idx < 0 ? str : str.Substring(0, idx);
    }


    public static string STTClean(this string str) {
        if(string.IsNullOrEmpty(str))
            return str;
        var sb = new StringBuilder();
        foreach(var ch in str) {
            if(!char.IsPunctuation(ch))
                sb.Append(char.ToLower(ch));
        }
        return sb.ToString().Trim();
    }


    // ur not gonna pronounce this
    public static string BypassToken => "xkcd";
}
