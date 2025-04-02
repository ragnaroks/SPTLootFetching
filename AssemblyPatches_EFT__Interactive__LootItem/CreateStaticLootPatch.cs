using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

namespace SPTLootFetching.AssemblyPatches_EFT__Interactive__LootItem {
    public class CreateStaticLootPatch : ModulePatch {
        protected override MethodBase GetTargetMethod () {
            return typeof(EFT.Interactive.LootItem).GetMethod(nameof(EFT.Interactive.LootItem.CreateStaticLoot), BindingFlags.Public | BindingFlags.Static);
        }

        [PatchPostfix]
        public static void Postfix (ref EFT.Interactive.LootItem __reslut) {
            Vector3 position = __reslut.gameObject.transform.position;
            Console.WriteLine("在 {0},{1},{2} 生成了松散战利品 {3}",position.x,position.y,position.z,__reslut.Name);
            //_ = __reslut.gameObject.AddComponent<LootNameLabelComponent>();
        }
    }
}
