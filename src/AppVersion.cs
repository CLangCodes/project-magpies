using System.Reflection;

namespace ProjectMagpie;

internal static class AppVersion
{
    // <Version> from Directory.Build.props; strip the "+<commit sha>" suffix .NET appends
    public static readonly string Value =
        typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            .Split('+')[0]
        ?? "0.0.0";
}
