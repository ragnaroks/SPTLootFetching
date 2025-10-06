using System;
using TMPro;
using UnityEngine;

namespace SPTLootFetching.UnityComponents {
    public class LootNameLabelComponent : MonoBehaviour {
        private EFT.Interactive.LootItem? LootItem { get; set; } = null;

        private GameObject LabelObject{get;set;} = new GameObject();

        private TextMeshPro? LabelComponent{get;set;} = null;

        public void Awake () {
            this.LootItem = this.gameObject.GetComponentInParent<EFT.Interactive.LootItem>();
            String lootName = String.Concat("<", String.IsNullOrWhiteSpace(this.LootItem.Name.Localized()) ? this.LootItem.Name : this.LootItem.Name.Localized(), ">");
            this.LabelObject.name = String.Concat("LootNameLabelObject",lootName);
            this.LabelObject.transform.SetParent(this.gameObject.transform);
            this.LabelComponent = this.LabelObject.GetOrAddComponent<TextMeshPro>();
            Color color = SPTLootFetchingPlugin.ESPColor?.Value ?? new Color(94, 136, 170);
            if (this.LootItem?.Item?.QuestItem ?? false) {
                color = new Color(255, 165, 0);
            }
            this.LabelComponent.alignment = TextAlignmentOptions.Center;
            this.LabelComponent.maxVisibleCharacters = 16;
            this.LabelComponent.maxVisibleLines = 1;
            this.LabelComponent.color = color;
            this.LabelComponent.fontSize = 0.5F;
            this.LabelComponent.enableAutoSizing = false;
            this.LabelComponent.fontWeight = FontWeight.Thin;
            this.LabelComponent.outlineWidth = 0.0625F;
            this.LabelComponent.isOverlay = true;
            //this.LabelComponent.isOrthographic = true;
            this.LabelComponent.text = lootName;
            this.LabelComponent.transform.localPosition = new Vector3(0,0.01F,0); // set anchor
            this.LabelComponent.autoSizeTextContainer = true;
        }

        //public void Start () {}

        //public void Update () {}

        public void LateUpdate () {
            if(this.LabelComponent==null || Camera.main==null){return;}
            Single limit = SPTLootFetchingPlugin.Distance?.Value ?? SPTLootFetchingPlugin.DefaultDistance;
            //Vector3 vector = this.gameObject.transform.position - Camera.main.gameObject.transform.position;
            Single vector = Vector3.Distance(this.gameObject.transform.position,Camera.main.gameObject.transform.position);
            //if (limit <= 0 || vector.sqrMagnitude > limit * limit) {return;}
            if (limit <= 0 || vector > limit) {return;}
            this.LabelComponent.transform.rotation = Camera.main.gameObject.transform.rotation;
            this.LabelComponent.fontSize = 0.5F + (vector * 0.10625F);
        }

        public void OnDestroy () {
            this.LootItem = null;
            this.LabelComponent = null;
            this.LabelObject.DestroyAllChildren();
        }
    }
}
