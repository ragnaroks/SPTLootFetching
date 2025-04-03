using SPT.Reflection.Patching;
using SPTLootFetching.UnityComponents;
using System.Reflection;

namespace SPTLootFetching.AssemblyPatches_EFT__Interactive__LootItem {
    public class InitPatch : ModulePatch {
        protected override MethodBase GetTargetMethod () {
            return typeof(EFT.Interactive.LootItem).GetMethod(nameof(EFT.Interactive.LootItem.Init), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void Postfix (ref EFT.Interactive.LootItem __instance) {
            if(Comfort.Common.Singleton<EFT.GameWorld>.Instance is HideoutGameWorld){return;}
            _ = __instance.gameObject.GetOrAddComponent<LootNameLabelComponent>();
        }
    }
}
