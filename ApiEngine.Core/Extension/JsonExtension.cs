using Furion.JsonSerialization;

namespace ApiEngine.Core.Extension;

public static class JsonExtension
{
    /// <summary>
    ///     将对象转化为json字符串
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string ToJson(this object obj)
    {
        return JSON.Serialize(obj);
    }

    /// <summary>
    ///     将json字符串转化为指定的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T JsonTo<T>(this string json) where T : class
    {
        return JSON.Deserialize<T>(json);
    }
}