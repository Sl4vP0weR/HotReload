namespace HotReload;

partial class HotReloader
{
    private static Assembly LoadAssembly(byte[] assemblyData, byte[] assemblySymbols = null)
    {
        var modCtx = ModuleDef.CreateModuleContext();
        var module = ModuleDefMD.Load(assemblyData, modCtx);
        var assemblyName = new AssemblyName(module.Assembly.FullName);

        var isStrongNamed = module.Assembly.PublicKey is not null;

        if (!UnderMonoRuntime() || isStrongNamed)
            goto defaultLoad;

        if (!Assemblies.FindOriginal(assemblyName).IsValid)
            goto defaultLoad;

        var guid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
        var name = $"{module.Assembly.Name}-{guid}";

        module.Assembly.Name = name;
        module.Assembly.PublicKey = null;
        module.Assembly.HasPublicKey = false;

        var options = new ModuleWriterOptions(module)
        {
            WritePdb = true,
            MetaDataOptions = new() // fix obfuscated assemblies issues
            {
                Flags =
                MetaDataFlags.KeepOldMaxStack |
                MetaDataFlags.PreserveAll |
                MetaDataFlags.AlwaysCreateStringsHeap |
                MetaDataFlags.AlwaysCreateUSHeap |
                MetaDataFlags.AlwaysCreateBlobHeap |
                MetaDataFlags.AlwaysCreateGuidHeap
            }
        };
        using (var output = new MemoryStream())
        {
            module.Write(output, options);
            assemblyData = output.GetBuffer();
        }

        defaultLoad:
        return Assembly.Load(assemblyData, assemblySymbols);
    }

    /// <summary>
    /// Tries to find assembly file from associated files extensions. Associated: .pdb|.exe|.dll
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static FileInfo TryFindAssemblyFile(FileInfo file)
    {
        if (file is null) return null;
        if (!file.Exists)
        {
            return TryFindAssemblyFile(
                file.Extension.ToLower() switch
                {
                    SymbolsFileExtension => file.WithExtension(LibraryAssemblyFileExtension),
                    LibraryAssemblyFileExtension => file.WithExtension(ExecutableAssemblyFileExtension),
                    _ => null
                }
            );
        }
        if (file.ExtensionIs(SymbolsFileExtension))
            return TryFindAssemblyFile(file.WithExtension(LibraryAssemblyFileExtension));
        try
        {
            if (AssemblyName.GetAssemblyName(file.FullName) is null)
                return null;
        }
        catch { file = null; }
        return file;
    }

    /// <summary>
    /// Loads [Re]Assembly from associated <paramref name="file"/>, handles and returns information about it.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="pdbFile"></param>
    public static AssemblyInfo Load(FileInfo file, FileInfo pdbFile = null)
    {
        file = TryFindAssemblyFile(file);
        if (file is null) return AssemblyInfo.Invalid;

        var path = file.FullName;
        var assemblyBytes = File.ReadAllBytes(path);

        pdbFile = (pdbFile?.Exists ?? false) ? pdbFile : new FileInfo(Path.ChangeExtension(path, SymbolsFileExtension));
        var pdbBytes = pdbFile.Exists ? File.ReadAllBytes(pdbFile.FullName) : null;

        Logger?.Invoke($"Loading [Re]Assembly {path}.");
        var assembly = LoadAssembly(assemblyBytes, pdbBytes);
        var info = HandleLoad(assembly, codeBase: file);
        return info;
    }

    /// <summary>
    /// Adds assembly info of <paramref name="assembly"/> to the assembly store and calls <see cref="TryUpdate"/>.
    /// </summary>
    /// <param name="assembly">Loaded HotReloadable assembly</param>
    /// <param name="name">Custom name [Unrecommended](Use only if you know what you're doing!)</param>
    /// <param name="codeBase">Custom CodeBase</param>
    public static AssemblyInfo HandleLoad(Assembly assembly, AssemblyName name = null, FileInfo codeBase = null)
    {
        var info = Assemblies.FindExact(assembly);
        if (info) return info;

        codeBase ??= new(assembly.CodeBase);
        if (!codeBase.Exists) return info;
        name ??= assembly.GetName();

        info = new(assembly, name, codeBase)
        {
            TimeStamp = DateTime.Now
        };

        var originalAssembly = Assemblies.FindOriginal(name);
        Assemblies.Add(info);
        try
        {
            if (originalAssembly.IsValid)
            {
                PreLoad?.Invoke(info);
                TryUpdate(originalAssembly, assembly);
                PostLoad?.Invoke(info);
            }
        }
        catch { }
        return info;
    }

    /// <summary>
    /// Searches for <see cref="HotReloadable"/> entries in <paramref name="newAssembly"/> and tries to replace them in <paramref name="originalAssembly"/>.
    /// </summary>
    /// <param name="originalAssembly"></param>
    /// <param name="newAssembly"></param>
    public static void TryUpdate(Assembly originalAssembly, Assembly newAssembly)
    {
        if (originalAssembly is null) return;

        var assemblyName = originalAssembly.GetName();

        Logger?.Invoke($"Updating assembly {assemblyName}.");

        foreach (var type in newAssembly.GetAllTypes())
        {
            foreach (var method in type.GetMethods(AccessTools.all))
            {
                var methodFullName = $"{type.FullName}.{method.Name}";
                try
                {
                    var attribute = method.GetCustomAttribute<HotReloadable>();
                    if (attribute is null) continue;

                    var originalType = originalAssembly.GetType(type.FullName);
                    var originalMethod = GetOriginalMethod(method, originalType);
                    if (originalMethod is null) continue;

                    Logger?.Invoke($"Updating newMethod {methodFullName}.");

                    var error = "";
                    try
                    {
                        PreUpdate?.Invoke(originalMethod, method);
                        error = Memory.DetourMethod(originalMethod, method);
                        PostUpdate?.Invoke(originalMethod, method);
                    }
                    catch { }
                    if(!string.IsNullOrWhiteSpace(error))
                        throw new Exception(error);
                }
                catch (Exception ex)
                {
                    ErrorLogger?.Invoke(new Exception($"Failed to update newMethod {methodFullName}.", ex));
                }
            }
        }
    }
}