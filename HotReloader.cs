namespace HotReload;

/// <summary>
/// Hot Reloading assemblies for Mono runtime.<br/>
/// Applies only on current <see cref="AppDomain"/>.<br/>
/// Handles [Re]Assemblies (Reloadable assemblies)
/// </summary>
public static partial class HotReloader
{
    /// <summary>
    /// Define <see langword="false" /> if you don't want to load default <see cref="Assembly"/> as [Re]<see cref="Assembly"/> on <see cref="Assembly.Load(string)"/> etc.
    /// </summary>
    public static bool HandleFileLoading = true;
    public static Action<object> 
        Logger = Console.WriteLine,
        ErrorLogger = Console.WriteLine;

    public static readonly AssemblyStore Assemblies = new();

    public static event AssemblyLoad PreLoad, PostLoad;
    public static event MethodUpdate PreUpdate, PostUpdate;

    public delegate void MethodUpdate(MethodInfo originalMethod, MethodInfo newMethod);
    public delegate void AssemblyLoad(AssemblyInfo info);

    public const string
        LibraryAssemblyFileExtension = ".dll",
        ExecutableAssemblyFileExtension = ".exe",
        SymbolsFileExtension = ".pdb";
}