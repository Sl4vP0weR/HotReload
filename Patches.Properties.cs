namespace HotReload;

partial class Patches
{
    partial class Assembly
    {
        [HarmonyPatch(typeof(System.Reflection.Assembly), nameof(GetName), new Type[0])]
        [HarmonyPostfix]
        public static void GetName(System.Reflection.Assembly __instance, ref AssemblyName __result) => GetRealName(__instance, ref __result);
        public static void GetRealName(System.Reflection.Assembly assembly, ref AssemblyName name)
        {
            var realName = HotReloader.Assemblies.FindOriginal(assembly).assemblyName;
            if (realName is not null)
                name = realName;
        }
        [HarmonyPatch(typeof(System.Reflection.Assembly), nameof(get_CodeBase), new Type[0])]
        [HarmonyPrefix]
        public static void get_CodeBase(System.Reflection.Assembly __instance, ref string __result) => GetCodeBase(__instance, ref __result);
        public static void GetCodeBase(System.Reflection.Assembly assembly, ref string codeBase)
        {
            var assemblyInfo = HotReloader.Assemblies.FindOriginal(assembly);
            var path = assemblyInfo.codeBase?.FullName;
            codeBase = string.IsNullOrWhiteSpace(path) ? codeBase : path;
        }
    }
}