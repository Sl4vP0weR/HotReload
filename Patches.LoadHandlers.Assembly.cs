namespace HotReload;

partial class Patches
{
    [Harmony]
    public static partial class Assembly
    {
        [HarmonyPatch(typeof(System.Reflection.Assembly), nameof(LoadFrom))]
        public static class LoadFrom
        {
            [HarmonyPatch(new[] { typeof(string) })]
            public static bool Prefix(ref System.Reflection.Assembly __result, string assemblyFile) =>
                HandleAssemblyLoad(ref __result, assemblyFile);

            [HarmonyPatch(new[] { typeof(string), typeof(Evidence) })]
            public static bool Prefix(ref System.Reflection.Assembly __result, string assemblyFile, Evidence securityEvidence) =>
                HandleAssemblyLoad(ref __result, assemblyFile, securityEvidence);
        }

        [HarmonyPatch(typeof(System.Reflection.Assembly), nameof(LoadFile))]
        public static class LoadFile
        {
            [HarmonyPatch(new[] { typeof(string) })]
            public static bool Prefix(ref System.Reflection.Assembly __result, string path) =>
                HandleAssemblyLoad(ref __result, path);

            [HarmonyPatch(new[] { typeof(string), typeof(Evidence) })]
            public static bool Prefix(ref System.Reflection.Assembly __result, string path, Evidence securityEvidence) =>
                HandleAssemblyLoad(ref __result, path, securityEvidence);
        }
    }
}