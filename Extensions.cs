global using static HotReload.Extensions;

namespace HotReload;

public static partial class Extensions
{
    public static MethodInfo GetOriginalMethod(MethodInfo newMethod, Type originalType) => 
        originalType.GetMethod(
            newMethod.Name,
            AccessTools.all,
            null,
            newMethod.GetParameters().Select(x => x.ParameterType).ToArray(),
            null
        );
    public static Type[] GetAllTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex) 
        {
            return ex.Types;
        }
    }
    public static FileInfo WithExtension(this FileInfo file, string extension) =>
        new(Path.ChangeExtension(file.FullName, extension));
    public static bool ExtensionIs(this FileInfo file, string extension) =>
        file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase);

    public static bool UnderMonoRuntime() => Type.GetType("Mono.Runtime") is not null;
}