namespace HotReload;

public sealed class AssemblyStore
{
    internal readonly List<AssemblyInfo> loaded = new();
    public IReadOnlyCollection<AssemblyInfo> Loaded => loaded;
    public AssemblyInfo FindExact(Assembly assembly) => loaded.FirstOrDefault(x => x.Assembly == assembly);
    public AssemblyInfo FindLatest(AssemblyInfo info) => Find(info, loaded.LastOrDefault);
    public AssemblyInfo FindOriginal(AssemblyInfo info) => Find(info, loaded.FirstOrDefault);
    internal void Add(AssemblyInfo info) => loaded.Add(info);

    internal delegate AssemblyInfo AssemblyGetter(Func<AssemblyInfo, bool> getter);

    internal AssemblyInfo Find(AssemblyInfo info, AssemblyGetter getter)
    {
        info = Find(info.assemblyName, getter);
        if (!info.IsValid) info = Find(info.CodeBase, getter);
        return info;
    }

    internal AssemblyInfo Find(AssemblyName name, AssemblyGetter getter)
    {
        if (name is null) return default;
        return getter(x => Predicate(x.assemblyName, name));
    }
    internal AssemblyInfo Find(FileInfo codeBase, AssemblyGetter getter)
    {
        if (codeBase is null) return default;
        return getter(x => Predicate(x.CodeBase, codeBase));
    }

    internal bool Predicate(AssemblyName name, AssemblyName originalName) =>
        name is not null &&
        name.Name == originalName.Name &&
        name.Version >= originalName.Version;
    internal bool Predicate(FileInfo file, FileInfo originalFile) =>
        file is not null &&
        file.FullName == originalFile.FullName;
}
