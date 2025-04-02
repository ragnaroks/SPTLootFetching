using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPTLootFetching {
    [BepInPlugin("net.skydust.SPTLootFetchingPlugin", "SPTLootFetchingPlugin", "1.0.1")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTLootFetchingPlugin : BaseUnityPlugin {        
        public static ConfigEntry<Boolean>? EnableESP{get;private set;} = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut{get;private set;} = null;

        private AssemblyPatches_EFT__Interactive__LootItem.InitPatch? InitPatch{get;set;} = null;

        private Boolean IsBusy{get;set;} = false;

        protected void Awake () {
            SPTLootFetchingPlugin.EnableESP = this.Config.Bind<Boolean>("config","enable-ESP",false,String.Empty);
            SPTLootFetchingPlugin.Shortcut = this.Config.Bind<KeyboardShortcut>("config","shortcut",new KeyboardShortcut(KeyCode.Slash,KeyCode.LeftControl),"press to fetching loose loots");
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
            this.IsBusy = true;
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            Vector3 position = gameWorld.MainPlayer.Transform.TransformPoint(new Vector3(0F,1F,2F));
            _ = this.StartCoroutine(this.TeleportLoots(gameWorld.LootItems.list_0,position));
        }

        protected void OnDestroy() {
            this.InitPatch?.Disable();
            this.Logger.LogDebug("plugin deactived");
        }

        private IEnumerator TeleportLoots (IList<EFT.Interactive.LootItem> loots, Vector3 position) {
            foreach (EFT.Interactive.LootItem lootItem in loots) {
                if(lootItem.gameObject.GetComponent<UnityComponents.LootNameLabelComponent>()==null){continue;}
                lootItem.transform.position = position + (UnityEngine.Random.onUnitSphere * 0.5F);
                yield return new WaitForSeconds(0.1F);// save CPU single core
            }
            NotificationManagerClass.DisplayMessageNotification(String.Concat(loots.Count," loots fetched"));
            this.IsBusy = false;
            yield break;
        }
    }
}
