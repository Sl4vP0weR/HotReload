namespace HotReload;

partial class HotReloader
{
    private static readonly Harmony harmony = new(nameof(HotReloader));

    internal static void InitializePatching()
    {
        try
        {
            harmony.PatchAll();
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }
        catch (Exception ex) 
        {
            ErrorLogger?.Invoke(ex);
        }
    }
    internal static void FinalizePatching()
    {
        harmony.UnpatchAll(harmony.Id);
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) => Assemblies.FindOriginal(new AssemblyName(args.Name));
}