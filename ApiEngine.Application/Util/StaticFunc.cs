using System.Security.Claims;
using ApiEngine.Application.Dto;
using Furion;
using Furion.DistributedIDGenerator;

namespace ApiEngine.Application.Util;

public class StaticFunc
{
    public static string Token(string key)
    {
        return App.User?.FindFirstValue(key);
    }

    public static string TokenUCode()
    {
        return Token(nameof(UserClass.Ucode));
    }

    public static string TokenUName()
    {
        return Token(nameof(UserClass.Uname));
    }

    /// <summary>
    ///     生成16位随机字符串
    /// </summary>
    /// <returns></returns>
    public static string GuidTo16String()
    {
        var i = IDGen.NextID().ToByteArray().Aggregate<byte, long>(1, (current, b) => current * (b + 1));
        return $"{i - DateTime.Now.Ticks:x}".ToUpper();
    }
}