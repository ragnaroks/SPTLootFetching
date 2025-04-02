using System;
using UnityEngine;

namespace SPTLootFetching.UnityComponents {
    public class LootNameLabelComponent : MonoBehaviour {
        public static GUIStyle GUIStyle{get;} = new GUIStyle(){
            normal = {
                textColor = new Color(0,255,0,153)
            }
        };
        
        private EFT.Interactive.LootItem? LootItem{get;set;}=null;

        public void Awake(){
            if(Comfort.Common.Singleton<EFT.GameWorld>.Instance is HideoutGameWorld){return;}
            this.LootItem = this.GetComponentInParent<EFT.Interactive.LootItem>();
        }

        public void Start () {
            //
        }

        public void Update() {
            //
        }

        public void OnGUI() {
            if(SPTLootFetchingPlugin.EnableESP?.Value!=true){return;}
            if(this.LootItem==null){return;}
            Vector3 position = Camera.main.WorldToViewportPoint(this.gameObject.transform.position);
            if(position.x>1F || position.x<0F || position.y>1F || position.y<0F || position.z<0F){return;}
            Vector2 positionUI = new Vector2(){
                x = position.x * Screen.width,
                y = (1F-position.y) * Screen.height
            };
            String name = String.IsNullOrWhiteSpace(this.LootItem.Name.Localized()) ? this.LootItem.Name : this.LootItem.Name.Localized();
            GUI.Label(new Rect(positionUI.x-48F,positionUI.y-12F,96F,24F),String.Concat("<",name,">"),LootNameLabelComponent.GUIStyle);
        }

        public void OnDestroy(){
            this.LootItem = null;
        }
    }
}
