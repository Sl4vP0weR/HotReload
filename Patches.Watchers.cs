namespace HotReload;

partial class Patches 
{
    [HarmonyPatch]
    public static partial class FileSystemEventArgs
    {
        public static MethodBase TargetMethod() => typeof(System.IO.FileSystemEventArgs).GetConstructors().FirstOrDefault();
        public static void Prefix(string directory, ref string name) // name may contain FullPath with directory, so we should remove directory
        {
            name = name.Replace(directory, "");
        }
    }
}