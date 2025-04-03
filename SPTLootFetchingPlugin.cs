using BepInEx;
using BepInEx.Configuration;
using EFT.Interactive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SPTLootFetching {
    [BepInPlugin("net.skydust.SPTLootFetchingPlugin", "SPTLootFetchingPlugin", "1.0.1")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTLootFetchingPlugin : BaseUnityPlugin {        
        public static ConfigEntry<KeyboardShortcut>? Shortcut{get;private set;} = null;

        public static ConfigEntry<Single>? Distance{get;private set;} = null;

        public static ConfigEntry<Boolean>? ESPEnable{get;private set;} = null;
        
        public static ConfigEntry<Color>? ESPColor{get;private set;} = null;

        private AssemblyPatches_EFT__Interactive__LootItem.InitPatch? InitPatch{get;set;} = null;

        private Boolean IsBusy{get;set;} = false;

        protected void Awake () {
            SPTLootFetchingPlugin.Shortcut = this.Config.Bind<KeyboardShortcut>("config","shortcut",new KeyboardShortcut(KeyCode.Slash,KeyCode.LeftControl),"press to fetching loose loots");
            SPTLootFetchingPlugin.Distance = this.Config.Bind<Single>("config","distance",256F,new ConfigDescription("will culling if distance above this value",new AcceptableValueRange<Single>(1F,1024F)));
            SPTLootFetchingPlugin.ESPEnable = this.Config.Bind<Boolean>("ESP","enable",false,String.Empty);
            SPTLootFetchingPlugin.ESPColor = this.Config.Bind<Color>("ESP","color",new Color(94, 230, 144),"loot name color, apply in next raid");
            this.InitPatch = new AssemblyPatches_EFT__Interactive__LootItem.InitPatch();
            this.Logger.LogDebug("plugin loaded");
        }

        protected void Start () {
            this.InitPatch?.Enable();
            this.Logger.LogDebug("plugin actived");
        }

        protected void Update () {
            if(this.IsBusy){return;}
            if(SPTLootFetchingPlugin.Shortcut?.Value.IsUp()!=true){return;}
            if(Comfort.Common.Singleton<EFT.GameWorld>.Instance==null){return;}
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if(gameWorld.MainPlayer==null){return;}
            this.IsBusy = true;
            Vector3 position = gameWorld.MainPlayer.Transform.TransformPoint(new Vector3(0F,1F,2F));
            Single distance = SPTLootFetchingPlugin.Distance?.Value ?? 256F;
            IList<EFT.Interactive.LootItem> loots = gameWorld.LootItems.list_0.FindAll(
                x=>x.isActiveAndEnabled 
                /*
                 * cause OutOfRangeException
                 * && x.IsVisibilityEnabled 
                */
                && x.IsValidForProfile(gameWorld.MainPlayer.ProfileId)
                && !x.Item.QuestItem
                && !x.Item.IsEmptyStack
                && Vector3.Distance(position,x.gameObject.transform.position)<=distance
            );
            _ = this.StartCoroutine(this.TeleportLoots(loots,position));
        }

        protected void OnDestroy() {
            this.InitPatch?.Disable();
            this.Logger.LogDebug("plugin deactived");
        }

        private IEnumerator TeleportLoots (IList<EFT.Interactive.LootItem> loots, Vector3 position) {
            foreach (EFT.Interactive.LootItem lootItem in loots) {
                lootItem.Shift = Vector3.zero;
                lootItem.transform.position = position + (UnityEngine.Random.onUnitSphere * 0.5F);
                yield return new WaitForSeconds(0.1F);// save CPU single core
            }
            NotificationManagerClass.DisplayMessageNotification(String.Concat(loots.Count," loots fetched"));
            this.IsBusy = false;
            yield break;
        }
    }
}
