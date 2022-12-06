namespace HotReload;

partial class HotReloader
{
    public static bool IsInitialized { get; private set; }
    static HotReloader()
    {
        Initialize();
    }
    public static void Initialize()
    {
        if (IsInitialized) return;
        InitializePatching();
        IsInitialized = true;
    }
#pragma warning disable CS0465 // Introducing a 'Finalize' method can interfere with destructor invocation
    public static void Finalize()
    {
        if (!IsInitialized) return;
        FinalizePatching();
        IsInitialized = false;
    }
#pragma warning restore CS0465
}