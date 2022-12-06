namespace HotReload;

partial class Patches
{
    [Harmony]
    public static partial class AppDomain
    {
        [HarmonyPatch(typeof(System.AppDomain), nameof(Load))]
        public static class Load
        {
            [HarmonyPatch(new[] { typeof(string) })]
            public static bool Prefix(ref System.Reflection.Assembly __result, string assemblyString) =>
                HandleAssemblyLoad(ref __result, assemblyString);

            [HarmonyPatch(new[] { typeof(string), typeof(Evidence) })]
            public static bool Prefix(ref System.Reflection.Assembly __result, string assemblyString, Evidence assemblySecurity) =>
                HandleAssemblyLoad(ref __result, assemblyString, assemblySecurity);
        }
    }
}