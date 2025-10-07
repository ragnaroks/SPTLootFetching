using SPT.Reflection.Patching;
using SPTLootFetching.UnityComponents;
using System.Reflection;

namespace SPTLootFetching.AssemblyPatches_EFT__Interactive__LootItem {
    public class CreateStationaryWeaponPatch : ModulePatch {
        protected override MethodBase GetTargetMethod () {
            return typeof(EFT.Interactive.LootItem).GetMethod(nameof(EFT.Interactive.LootItem.CreateStationaryWeapon), BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPostfix]
        public static void Postfix (EFT.Interactive.StationaryWeapon stationaryWeapon) {
            _ = stationaryWeapon.gameObject.AddComponent<LootNameLabelComponent>();
        }
    }
}
