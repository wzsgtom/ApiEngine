namespace ApiEngine.Core.Extensions;

public static class JsonExtension
{
    /// <summary>
    ///     获取指定的Value：通过使用区分区域性的排序规则、当前区域性，并忽略所比较的字符串的大小写，来比较字符串
    /// </summary>
    /// <returns></returns>
    public static string GetValueIgnoreCase(this JObject jobj, string key, string defaultvalue = "")
    {
        return jobj.GetValue(key, StringComparison.CurrentCultureIgnoreCase).ObjToString(defaultvalue);
    }

    /// <summary>
    ///     将对象转化为json字符串
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string ToJson(this object obj)
    {
        return JSON.GetJsonSerializer().Serialize(obj);
    }

    /// <summary>
    ///     将json字符串转化为指定的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T JsonTo<T>(this string json) where T : class
    {
        return JSON.GetJsonSerializer().Deserialize<T>(json);
    }
}