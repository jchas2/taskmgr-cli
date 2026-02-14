using System.Reflection;

namespace Task.Manager.Tests.Common;

public static class CanaryTestHelper
{
    public static int GetProperties<T>() => GetProperties<T>(BindingFlags.Instance | BindingFlags.Public);

    public static int GetProperties<T>(BindingFlags flags)
    {
        PropertyInfo[] pinfos = typeof(T).GetProperties(flags);
        return pinfos.Length;
    }
}
