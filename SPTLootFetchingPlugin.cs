﻿using BepInEx;
using BepInEx.Configuration;
using EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPTLootFetching {
    [BepInPlugin("net.skydust.SPTLootFetchingPlugin", "SPTLootFetchingPlugin", "1.0.4")]
    [BepInProcess("EscapeFromTarkov")]
    public class SPTLootFetchingPlugin : BaseUnityPlugin {
        public static ConfigEntry<KeyboardShortcut>? Shortcut { get; private set; } = null;

        public static ConfigEntry<Single>? Distance { get; private set; } = null;

        public static ConfigEntry<Boolean>? ESPEnable { get; private set; } = null;

        public static ConfigEntry<Color>? ESPColor { get; private set; } = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut2 { get; private set; } = null;

        public static ConfigEntry<KeyboardShortcut>? Shortcut3 { get; private set; } = null;

        public static Single DefaultDistance{get;} = 256F;

        private AssemblyPatches_EFT__Interactive__LootItem.InitPatch? InitPatch { get; set; } = null;

        private Boolean IsBusy { get; set; } = false;

        protected void Awake () {
            SPTLootFetchingPlugin.Shortcut = this.Config.Bind<KeyboardShortcut>("config", "shortcut", new KeyboardShortcut(KeyCode.Comma, KeyCode.LeftControl), "press to fetching loose loots in distance range");
            SPTLootFetchingPlugin.Shortcut2 = this.Config.Bind<KeyboardShortcut>("config", "shortcut-2", new KeyboardShortcut(KeyCode.Period, KeyCode.LeftControl), "press to fetching loose loots in whole map");
            SPTLootFetchingPlugin.Shortcut3 = this.Config.Bind<KeyboardShortcut>("config", "shortcut-3", new KeyboardShortcut(KeyCode.Slash, KeyCode.LeftControl), "press to fetching bots in whole map");
            SPTLootFetchingPlugin.Distance = this.Config.Bind<Single>("config", "distance", SPTLootFetchingPlugin.DefaultDistance, new ConfigDescription("will culling if distance above this value", new AcceptableValueRange<Single>(1F, 1024F)));
            SPTLootFetchingPlugin.ESPEnable = this.Config.Bind<Boolean>("ESP", "enable", false, String.Empty);
            SPTLootFetchingPlugin.ESPColor = this.Config.Bind<Color>("ESP", "color", new Color(94, 230, 144), "loot name color, apply in next raid");
            this.InitPatch = new AssemblyPatches_EFT__Interactive__LootItem.InitPatch();
            this.Logger.LogDebug("plugin loaded");
        }

        protected void Start () {
            this.InitPatch?.Enable();
            this.Logger.LogDebug("plugin actived");
        }

        protected void Update () {
            if (this.IsBusy) { return; }
            if (SPTLootFetchingPlugin.Shortcut?.Value.IsUp() == true) {
                this.FetchInRange(SPTLootFetchingPlugin.Distance?.Value ?? SPTLootFetchingPlugin.DefaultDistance);
                return;
            }
            if (SPTLootFetchingPlugin.Shortcut2?.Value.IsUp() == true) {
                this.FetchInRange(null);
                return;
            }
            if (SPTLootFetchingPlugin.Shortcut3?.Value.IsUp() == true) {
                this.FetchBots();
                return;
            }
        }

        protected void OnDestroy () {
            this.InitPatch?.Disable();
            this.Logger.LogDebug("plugin deactived");
        }

        private void FetchInRange (Single? distance) {
            if (this.IsBusy) { return; }
            if (Comfort.Common.Singleton<EFT.GameWorld>.Instance == null) { return; }
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld.MainPlayer == null) { return; }
            this.IsBusy = true;
            Vector3 position = gameWorld.MainPlayer.Transform.TransformPoint(new Vector3(0F, 1F, 2F));
            List<EFT.Interactive.LootItem> loots = gameWorld.LootItems.list_0.FindAll(
                x => x.isActiveAndEnabled
                // cause OutOfRangeException
                // && x.IsVisibilityEnabled
                && x.IsValidForProfile(gameWorld.MainPlayer.ProfileId)
                && !x.Item.QuestItem
                && !x.Item.IsEmptyStack
            );
            if (distance.HasValue) {
                // loots = loots.FindAll(x => Vector3.Distance(position, x.transform.position) <= distance);
                loots = loots.FindAll(x => (x.transform.position - position).sqrMagnitude <= distance * distance);
            }
            _ = this.StartCoroutine(this.TeleportLoots(loots, position));
        }

        private IEnumerator TeleportLoots (List<EFT.Interactive.LootItem> loots, Vector3 position) {
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

        private void FetchBots () {
            if (Comfort.Common.Singleton<EFT.GameWorld>.Instance == null) { return; }
            EFT.GameWorld gameWorld = Comfort.Common.Singleton<EFT.GameWorld>.Instance;
            if (gameWorld.MainPlayer == null) { return; }
            this.IsBusy = true;
            Vector3 position = gameWorld.MainPlayer.Transform.TransformPoint(new Vector3(0F, 1F, 2F));
            EFT.Player? singleBot = gameWorld.AllAlivePlayersList.Find(
                x => x.ProfileId != gameWorld.MainPlayer.ProfileId
                && !x.IsYourPlayer
                && x.IsAI
                && x.gameObject.activeInHierarchy
                //&& x.GetComponent<EFT.EftGamePlayerOwner>()
                && x.GetComponent<EFT.Interactive.Corpse>() == null
                && x.GetComponent<EFT.LocalPlayer>() != null
                && x.GetComponent<EFT.BotOwner>() != null
            );
            if(singleBot==null){
                this.IsBusy = false;
                return;
            }
            _ = this.StartCoroutine(this.TeleportBots(new List<EFT.Player> { singleBot }, position));
        }

        private IEnumerator TeleportBots (List<EFT.Player> bots, Vector3 position) {
            foreach (EFT.Player bot in bots) {
                bot.Teleport(position);
                bot.GetComponent<EFT.BotOwner>().Settings.botDifficulty_0 = BotDifficulty.impossible;
                NotificationManagerClass.DisplayMessageNotification(String.Concat("bot <",bot.name, "> fetched"));
                yield return new WaitForSeconds(0.1F);// save CPU single core
            }
            //NotificationManagerClass.DisplayMessageNotification(String.Concat(bots.Count, " bots fetched"));
            this.IsBusy = false;
            yield break;
        }
    }
}
