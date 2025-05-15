using System;
using UnityEngine;

namespace SPTLootFetching.UnityComponents {
    public class LootNameLabelComponent : MonoBehaviour {
        public static GUIStyle GUIStyle { get; } = new GUIStyle() {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = {
                textColor = SPTLootFetchingPlugin.ESPColor?.Value ?? new Color(94, 136, 170),
            }
        };

        public static GUIStyle GUIStyle2 { get; } = new GUIStyle() {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            normal = {
                textColor = new Color(226, 207, 58),
            }
        };

        private EFT.Interactive.LootItem? LootItem { get; set; } = null;

        private String? LootName { get; set; } = String.Empty;

        private Vector3 ViewportPoint { get; set; } = Vector3.zero;

        private Vector3 ScreenPoint { get; set; } = Vector3.zero;

        private Vector2 TextPosition { get; set; } = Vector2.zero;

        public void Awake () {
            this.LootItem = this.GetComponentInParent<EFT.Interactive.LootItem>();
            this.LootName = String.IsNullOrWhiteSpace(this.LootItem.Name.Localized()) ? this.LootItem.Name : this.LootItem.Name.Localized();
        }

        public void Start () {
            //
        }

        public void Update () {
            this.TextPosition = Vector2.zero;
            if (SPTLootFetchingPlugin.ESPEnable?.Value != true) {return;}
            if (this.LootItem == null || Camera.main == null) {return;}
            Vector3 vector = this.gameObject.transform.position - Camera.main.gameObject.transform.position;
            Single limit = SPTLootFetchingPlugin.Distance?.Value ?? SPTLootFetchingPlugin.DefaultDistance;
            if (limit <= 0 || vector.sqrMagnitude > limit * limit) {return;}
            // world to screen => https://discussions.unity.com/t/gui-label-follow-an-object/29578/2
            this.ViewportPoint = Camera.main.WorldToViewportPoint(this.gameObject.transform.position);
            if (this.ViewportPoint.x > 1F || this.ViewportPoint.x < 0F || this.ViewportPoint.y > 1F || this.ViewportPoint.y < 0F || this.ViewportPoint.z < 0F) {return;}
            this.ScreenPoint = Camera.main.ViewportToScreenPoint(this.ViewportPoint);
            this.TextPosition = new Vector2(this.ScreenPoint.x,Screen.height - this.ScreenPoint.y);
        }

        public void OnGUI () {
            if (this.TextPosition == Vector2.zero) { return; }
            GUI.Label(
                new Rect(this.TextPosition.x - 64F, this.TextPosition.y - 12F, 128F, 24F),
                String.Concat("<", this.LootName, ">"),
                this.LootItem?.Item.QuestItem == true ? LootNameLabelComponent.GUIStyle2 : LootNameLabelComponent.GUIStyle
            );
        }

        public void OnDestroy () {
            this.LootItem = null;
            this.LootName = null;
        }
    }
}
