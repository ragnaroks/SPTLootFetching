using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SPTLootFetching {
    [BepInPlugin("net.skydust.sptlootfetchingp", "SPTLootFetchingPlugin", "1.0.8")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTLootFetchingPlugin : BaseUnityPlugin {
        public static ConfigEntry<KeyboardShortcut>? Shortcut1 { get; private set; } = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut2 { get; private set; } = null;

        public static ConfigEntry<Single>? Distance { get; private set; } = null;

        public static ConfigEntry<Boolean>? ESPEnable { get; private set; } = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut3 { get; private set; } = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut4 { get; private set; } = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut5 { get; private set; } = null;

        private AssemblyPatches_EFT__Interactive__LootItem.InitPatch InitPatch { get; } = new AssemblyPatches_EFT__Interactive__LootItem.InitPatch();

        private AssemblyPatches_EFT__Interactive__LootItem.CreateStaticLootPatch CreateStaticLootPatch { get; } = new AssemblyPatches_EFT__Interactive__LootItem.CreateStaticLootPatch();

        private AssemblyPatches_EFT__Interactive__LootItem.CreateLootWithRigidbodyPatch CreateLootWithRigidbodyPatch { get; } = new AssemblyPatches_EFT__Interactive__LootItem.CreateLootWithRigidbodyPatch();

        private AssemblyPatches_EFT__Interactive__LootItem.CreateStationaryWeaponPatch CreateStationaryWeaponPatch { get; } = new AssemblyPatches_EFT__Interactive__LootItem.CreateStationaryWeaponPatch();

        private Boolean IsBusy { get; set; } = false;

        protected void Awake () {
            SPTLootFetchingPlugin.Shortcut1 = this.Config.Bind<KeyboardShortcut>(
                "general",
                "shortcut-1",
                new KeyboardShortcut(KeyCode.Comma, KeyCode.LeftControl),
                "press to fetching loose loots in distance range"
            );
            SPTLootFetchingPlugin.Shortcut2 = this.Config.Bind<KeyboardShortcut>(
                "general",
                "shortcut-2",
                new KeyboardShortcut(KeyCode.Period, KeyCode.LeftControl),
                "press to fetching loose loots in whole map"
            );
            SPTLootFetchingPlugin.Distance = this.Config.Bind<Single>(
                "general",
                "distance",
                32F,
                new ConfigDescription(
                    "will culling if distance above this value",
                    new AcceptableValueRange<Single>(1F, 1024F)
                )
            );
            SPTLootFetchingPlugin.ESPEnable = this.Config.Bind<Boolean>(
                "general",
                "esp-enable",
                true,
                String.Empty
            );
            SPTLootFetchingPlugin.Shortcut3 = this.Config.Bind<KeyboardShortcut>(
                "tools",
                "shortcut-3",
                new KeyboardShortcut(KeyCode.Keypad3, KeyCode.LeftControl),
                "teleport a nearest bot around to you"
            );
            SPTLootFetchingPlugin.Shortcut4 = this.Config.Bind<KeyboardShortcut>(
                "tools",
                "shortcut-4",
                new KeyboardShortcut(KeyCode.Keypad4, KeyCode.LeftControl),
                "teleport you around to nearest bot"
            );
            SPTLootFetchingPlugin.Shortcut5 = this.Config.Bind<KeyboardShortcut>(
                "tools",
                "shortcut-5",
                new KeyboardShortcut(KeyCode.Keypad5, KeyCode.LeftControl),
                "teleport you to lighthouse area, only work in lighthouse map"
            );
            this.Logger.LogDebug("plugin loaded");
        }

        protected void Start () {
            //this.CreateStaticLootPatch.Enable();
            //this.CreateLootWithRigidbodyPatch.Enable();
            //this.CreateStationaryWeaponPatch.Enable();
            this.InitPatch.Enable();
            this.Logger.LogDebug("plugin actived");
        }

        protected void Update () {
            if (this.IsBusy) { return; }
            if (SPTLootFetchingPlugin.Shortcut1?.Value.IsUp() == true) {
                this.FetchInRange(SPTLootFetchingPlugin.Distance?.Value);
                return;
            }
            if (SPTLootFetchingPlugin.Shortcut2?.Value.IsUp() == true) {
                this.FetchInRange(null);
                return;
            }
            if (SPTLootFetchingPlugin.Shortcut3?.Value.IsUp() == true) {
                this.TeleportBotToPlayer();
                return;
            }
            if (SPTLootFetchingPlugin.Shortcut4?.Value.IsUp() == true) {
                this.TeleportPlayerToBot();
                return;
            }
            if (SPTLootFetchingPlugin.Shortcut5?.Value.IsUp() == true) {
                this.TeleportPlayerToLighthouse();
                return;
            }
        }

        protected void OnDestroy () {
            //this.CreateStaticLootPatch.Disable();
            //this.CreateLootWithRigidbodyPatch.Disable();
            //this.CreateStationaryWeaponPatch.Disable();
            this.InitPatch.Disable();
            this.Logger.LogDebug("plugin deactived");
        }

        private void FetchInRange (Single? distance) {
            if(!distance.HasValue || distance<=0F){return;}
            if (this.IsBusy) { return; }
            if (Comfort.Common.Singleton<EFT.GameWorld>.Instance == null) { return; }
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld.MainPlayer == null) { return; }
            this.IsBusy = true;
            Vector3 position = gameWorld.MainPlayer.Transform.TransformPoint(new Vector3(0F, 1F, 2F));
            IEnumerable<EFT.Interactive.LootItem> loots = gameWorld.LootItems.List_0.Where(
                x => x.isActiveAndEnabled
                // cause OutOfRangeException
                // && x.IsVisibilityEnabled
                && x.IsValidForProfile(gameWorld.MainPlayer.ProfileId)
                && !x.Item.QuestItem
                && !x.Item.IsEmptyStack
            );
            if (distance.HasValue) {
                loots = loots.Where(x => (x.transform.position - position).sqrMagnitude <= distance * distance);
            }
            // ToList() create a copy to avoid "collection modified" error
            _ = this.StartCoroutine(this.TeleportLoots(loots.ToList(), position));
        }

        private IEnumerator TeleportLoots (IList<EFT.Interactive.LootItem> loots, Vector3 position) {
            foreach (EFT.Interactive.LootItem lootItem in loots) {
                lootItem.Shift = Vector3.zero;
                lootItem.transform.position = position + (UnityEngine.Random.onUnitSphere * 0.5F);
                if (lootItem is EFT.Interactive.Corpse) {
                    // fix corpse model is far away of position
                    lootItem.TrackableTransform.localPosition = Vector3.zero;
                    lootItem.TrackableTransform.localRotation = Quaternion.identity;
                }
                yield return new WaitForSeconds(0.1F);// save CPU single core
            }
            NotificationManagerClass.DisplayMessageNotification(String.Concat(loots.Count, " loots fetched"));
            this.IsBusy = false;
            yield break;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式", Justification = "<挂起>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0053:使用表达式主体来表示 Lambda 表达式", Justification = "<挂起>")]
        private void TeleportBotToPlayer () {
            if (this.IsBusy) { return; }
            if (Comfort.Common.Singleton<EFT.GameWorld>.Instance == null) { return; }
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld.AllAlivePlayersList.Count < 1) { return; }
            if (gameWorld.MainPlayer == null) { return; }
            this.IsBusy = true;
            Vector3 position = gameWorld.MainPlayer.Transform.TransformPoint(new Vector3(0F, 2F, 2F));
            EFT.Player? singleBot = gameWorld.AllAlivePlayersList.Where(item => {
                if (item.IsYourPlayer || item.ProfileId == gameWorld.MainPlayer.ProfileId) { return false; }
                if (!item.IsAI) { return false; }
                if (!item.gameObject.activeInHierarchy) { return false; }
                if (item.GetComponent<EFT.Interactive.Corpse>() != null) { return false; }
                if (item.GetComponent<EFT.LocalPlayer>() == null) { return false; }
                if (item.GetComponent<EFT.BotOwner>() == null) { return false; }
                if (item.GetComponent<AIFirearmController>()?.Item?.TemplateId == "657857faeff4c850222dff1b") { return false; }
                return true;
            }).OrderBy(item => {
                return (item.gameObject.transform.position - position).sqrMagnitude;
            }).FirstOrDefault();
            if (singleBot == null) {
                this.IsBusy = false;
                return;
            }
            singleBot.GetComponent<EFT.BotOwner>()?.StopMove();
            //botOwner.Mover.GoToPointNoWay(position);
            singleBot.Teleport(position);
            NotificationManagerClass.DisplayMessageNotification(String.Concat("bot <", singleBot.name, "> is around you"));
            this.IsBusy = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:转换为条件表达式", Justification = "<挂起>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0053:使用表达式主体来表示 Lambda 表达式", Justification = "<挂起>")]
        private void TeleportPlayerToBot () {
            if (this.IsBusy) { return; }
            if (Comfort.Common.Singleton<EFT.GameWorld>.Instance == null) { return; }
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld.AllAlivePlayersList.Count < 1) { return; }
            if (gameWorld.MainPlayer == null) { return; }
            this.IsBusy = true;
            Vector3 position = gameWorld.MainPlayer.Transform.position;
            EFT.Player? singleBot = gameWorld.AllAlivePlayersList.Where(item => {
                if (item.IsYourPlayer || item.ProfileId == gameWorld.MainPlayer.ProfileId) { return false; }
                if (!item.IsAI) { return false; }
                //if (!item.gameObject.activeInHierarchy) { return false; }
                //if (item.GetComponent<EFT.Interactive.Corpse>() != null) { return false; }
                if (item.GetComponent<EFT.LocalPlayer>() == null) { return false; }
                if (item.GetComponent<EFT.BotOwner>() == null) { return false; }
                if (item.GetComponent<AIFirearmController>()?.Item?.TemplateId == "657857faeff4c850222dff1b") { return false; }
                return true;
            }).OrderBy(item => {
                return (item.gameObject.transform.position - position).sqrMagnitude;
            }).FirstOrDefault();
            if (singleBot == null) {
                this.IsBusy = false;
                return;
            }
            gameWorld.MainPlayer.Teleport(singleBot.Transform.position);
            NotificationManagerClass.DisplayMessageNotification(String.Concat("you are around bot <", singleBot.name, ">"));
            this.IsBusy = false;
        }

        private void TeleportPlayerToLighthouse () {
            if (this.IsBusy) { return; }
            if (Comfort.Common.Singleton<EFT.GameWorld>.Instance == null) { return; }
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld.MainPlayer == null) { return; }
            String sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Lighthouse_Scripts") {
                NotificationManagerClass.DisplayMessageNotification("current map is not LightHouse");
                return;
            }
            this.IsBusy = true;
            gameWorld.MainPlayer.Teleport(new Vector3(174.25F, 2.5F, 522.35F), true);
            this.IsBusy = false;
        }
    }
}
