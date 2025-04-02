using SPT.Reflection.Patching;
using SPTLootFetching.UnityComponents;
using System.Reflection;

namespace SPTLootFetching.AssemblyPatches_EFT__Interactive__LootItem {
    public class CreateStaticLootPatch : ModulePatch {
        /// <summary>check => https://github.com/pardeike/Harmony/issues/121</summary>
        protected override MethodBase GetTargetMethod () {
            return typeof(EFT.Interactive.LootItem)
                .GetMethod(nameof(EFT.Interactive.LootItem.CreateStaticLoot), BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(typeof(EFT.Interactive.LootItem));
        }

        [PatchPostfix]
        public static void Postfix (ref EFT.Interactive.LootItem __result) {
            _ = __result.gameObject.AddComponent<LootNameLabelComponent>();
        }
    }
}
