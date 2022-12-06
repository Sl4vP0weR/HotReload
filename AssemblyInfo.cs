namespace HotReload;

public record struct AssemblyInfo
{
    internal AssemblyName assemblyName;
    public AssemblyName AssemblyName => assemblyName ??= Assembly?.GetName();
    internal FileInfo codeBase;
    public FileInfo CodeBase => codeBase ??= string.IsNullOrWhiteSpace(Assembly?.CodeBase) ? null : new(Assembly.CodeBase);
    public Assembly Assembly;
    public DateTime TimeStamp { get; internal set; }
    public bool HasReference => AssemblyName is not null || CodeBase is not null;
    public bool IsLoaded => Assembly is not null;
    public bool IsValid => IsLoaded || HasReference;

    public AssemblyInfo(Assembly assembly = null, AssemblyName assemblyName = null, FileInfo codeBase = null)
    {
        Assembly = assembly;
        this.assemblyName = assemblyName;
        this.codeBase = codeBase;
    }
    public static implicit operator AssemblyInfo(AssemblyName name) => new(assemblyName: name);
    public static implicit operator AssemblyInfo(FileInfo file) => new(codeBase: file);
    public static implicit operator AssemblyInfo(Assembly assembly) => new(assembly);
    public static implicit operator Assembly(AssemblyInfo info) => info.Assembly;
    public static implicit operator bool(AssemblyInfo info) => info.IsValid;

    public static AssemblyInfo Invalid => new();
}
