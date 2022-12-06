namespace HotReload;

partial class Patches
{
    public static bool HandleAssemblyLoad(ref System.Reflection.Assembly assembly, string assemblyFile)
    {
        if (!HotReloader.HandleFileLoading) goto defaultLoad;

        FileInfo file = new(assemblyFile);
        if (!file.Exists) goto defaultLoad;
        assembly = HotReloader.Load(file);

        defaultLoad:
        return assembly is null;
    }
    public static bool HandleAssemblyLoad(ref System.Reflection.Assembly assembly, string assemblyFile, Evidence securityEvidence)
    {
        var defaultLoad = HandleAssemblyLoad(ref assembly, assemblyFile);

        if (assembly is not null && securityEvidence is not null) // try merge evidence
            assembly.Evidence.Merge(securityEvidence);

        return defaultLoad;
    }
}