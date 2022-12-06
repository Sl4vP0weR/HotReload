namespace HotReload;

partial class Patches
{
    [HarmonyPatch(typeof(dnlib.DotNet.MD.ImageCor20Header))]
    public static partial class ImageCor20Header
    {
        /// <summary>
        /// Patch that sets cb header for all assemblies to 0x48 or higher(minimum requirement)
        /// </summary>
        [HarmonyPatch(nameof(get_CB))]
        [HarmonyPrefix]
        internal static bool get_CB(ref uint __result)
        {
            uint min = 0x48;
            if (__result < min) __result = min;
            return false;
        }
        [HarmonyPatch]
        public static class Constructor
        {
            public static MethodBase TargetMethod() => typeof(dnlib.DotNet.MD.ImageCor20Header).GetConstructors().FirstOrDefault();
            public static void Prefix(ref bool verify) => verify = false;
        }
    }
}