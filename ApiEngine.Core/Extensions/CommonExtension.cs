namespace ApiEngine.Core.Extensions;

public static class CommonExtension
{
    /// <summary>
    ///     将字符串转化为指定的类型，如果是null或空，则按照指定的默认值进行转化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static T ConvertType<T>(this string val)
    {
        if (typeof(T) != typeof(bool)) return (T)Convert.ChangeType(val, typeof(T));

        if (val.IsNullOrEmpty() || val.ToLower() == "false" || val == "0") val = "False";
        else val = "True";

        return (T)Convert.ChangeType(val, typeof(T));
    }

    public static string ToBase64String(this string value)
    {
        if (value.IsNullOrEmpty()) return "";

        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    public static string UnBase64String(this string value)
    {
        if (value.IsNullOrEmpty()) return "";

        var bytes = Convert.FromBase64String(value);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    ///     转换为大驼峰
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToPascal(this string value)
    {
        var split = value.Split('/', ' ', '-', '_', '.');
        var newStr = "";
        foreach (var item in split)
        {
            var chars = item.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            for (var i = 1; i < chars.Length; i++) chars[i] = char.ToLower(chars[i]);

            newStr += new string(chars);
        }

        return newStr;
    }

    public static string StringFormat(this string value, params object[] strings)
    {
        return string.Format(value, strings);
    }

    public static string StringJoin(this IEnumerable<object> enumerable, string separator)
    {
        return string.Join(separator, enumerable);
    }

    public static bool ContainsIgnoreCase(this string source, string substring)
    {
        return source?.IndexOf(substring, StringComparison.OrdinalIgnoreCase) > -1;
    }

    public static bool ContainsIgnoreCase(this List<string> list, string substring)
    {
        return list.FindAll(s => s.ContainsIgnoreCase(substring)).Count > 0;
    }

    public static (bool, string) Convert2DateString(this string value, string inFormat, string outFormat)
    {
        return DateTime.TryParseExact(value, inFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt)
            ? (true, dt.ToString(outFormat))
            : (false, null);
    }

    public static string Convert2DateString(this DateTime? date, string outFormat)
    {
        var outDate = DateTime.MinValue;
        if (date != null) outDate = (DateTime)date;

        return outDate.ToString(outFormat);
    }
}