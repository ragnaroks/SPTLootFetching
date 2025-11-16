using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using TMPro;
using UnityEngine;

namespace SPTLootFetching.UnityComponents {
    public class LootNameLabelComponent : MonoBehaviour {
        private LootItem? LootItem { get; set; } = null;

        private GameObject LabelObject { get; set; } = new GameObject();

        private TextMeshPro? LabelComponent { get; set; } = null;

        public void Awake () {
            this.LootItem = this.gameObject.GetComponentInParent<LootItem>();
            if (this.LootItem == null) { return; }
            String lootName = String.Concat("<", this.LootItem.Item.LocalizedShortName(), ">");
            this.LabelObject.name = String.Concat("LootNameLabelObject", lootName);
            this.LabelObject.transform.SetParent(this.gameObject.transform);
            this.LabelComponent = this.LabelObject.GetOrAddComponent<TextMeshPro>();
            Color color = Color.cyan;
            if (Singleton<GameWorld>.Instance.MainPlayer.Profile.WishlistManager.IsInWishlist(this.LootItem.Item.TemplateId,true,out _)) {
                color = Color.magenta;
            }
            if (this.LootItem?.Item?.QuestItem == true) {
                color = Color.yellow;
            }
            this.LabelComponent.alignment = TextAlignmentOptions.Center;
            this.LabelComponent.maxVisibleCharacters = 16;
            this.LabelComponent.maxVisibleLines = 1;
            this.LabelComponent.richText = false;
            this.LabelComponent.color = color;
            this.LabelComponent.fontSize = 0.25F;
            this.LabelComponent.enableAutoSizing = false;
            this.LabelComponent.fontWeight = FontWeight.Thin;
            this.LabelComponent.outlineWidth = 0.0625F;
            this.LabelComponent.isOverlay = true;
            //this.LabelComponent.isOrthographic = true;
            this.LabelComponent.text = lootName;
            this.LabelComponent.transform.localPosition = new Vector3(0, 0.15F, 0); // set anchor
            this.LabelComponent.autoSizeTextContainer = true;
        }

        //public void Start () {}

        //public void Update () {}

        public void LateUpdate () {
            if (this.LabelComponent == null || Camera.main == null) { return; }
            Boolean active;
            if (this.gameObject.GetComponent<Corpse>() != null && this.gameObject.GetComponent<BotOwner>() != null) {
                active = true;
            } else {
                active = this.gameObject.GetComponent<ObservedLootItem>()?.enabled == true;
            }
            this.LabelObject.SetActive(active);
            if (!active) { return; }
            Single limit = SPTLootFetchingPlugin.Distance?.Value ?? 0F;
            if (limit <= 0) { return; }
            Single distance = Vector3.Distance(this.gameObject.transform.position, Camera.main.gameObject.transform.position);
            if (distance > limit) { return; }
            this.LabelComponent.transform.rotation = Camera.main.gameObject.transform.rotation;
            this.LabelComponent.fontSize = 0.25F + (distance * 0.0625F);
        }

        public void OnDestroy () {
            this.LootItem = null;
            this.LabelComponent = null;
            if (this.gameObject != null) { Destroy(this.gameObject); }
        }
    }
}
