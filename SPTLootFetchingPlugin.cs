using BepInEx;
using BepInEx.Configuration;
using System;
using UnityEngine;

namespace SPTLootFetching {
    [BepInPlugin("net.skydust.SPTLootFetchingPlugin", "SPTLootFetchingPlugin", "1.0.1")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTLootFetchingPlugin : BaseUnityPlugin {        
        public static ConfigEntry<Boolean>? Debug{get;private set;} = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut{get;private set;} = null;

        private AssemblyPatches_EFT__Interactive__LootItem.CreateStaticLootPatch? CreateStaticLootPatch{get;set;} = null;

        protected void Awake () {
            SPTLootFetchingPlugin.Debug = this.Config.Bind<Boolean>("config","debug",false,"don't touch this");
            SPTLootFetchingPlugin.Shortcut = this.Config.Bind<KeyboardShortcut>("config","shortcut",new KeyboardShortcut(KeyCode.Slash,KeyCode.LeftControl),"press to fetching loose loots");
            this.CreateStaticLootPatch = new AssemblyPatches_EFT__Interactive__LootItem.CreateStaticLootPatch();
            this.Logger.LogDebug("plugin loaded");
        }

        protected void Start () {
            this.CreateStaticLootPatch?.Enable();
            this.Logger.LogDebug("plugin actived");
        }

        protected void Update () {
            //
        }

        protected void OnDestroy() {
            this.CreateStaticLootPatch?.Disable();
            this.Logger.LogDebug("plugin deactived");
        }
    }
}
