using System;
using UnityEngine;

namespace SPTLootFetching.UnityComponents {
    public class LootNameLabelComponent : MonoBehaviour {
        public static GUIStyle GUIStyle{get;} = new GUIStyle(){
            fontSize = 12,
            normal = {
                textColor = SPTLootFetchingPlugin.ESPColor?.Value ?? new Color(94, 136, 170),
            }            
        };
        
        private EFT.Interactive.LootItem? LootItem{get;set;}=null;

        public void Awake(){
            this.LootItem = this.GetComponentInParent<EFT.Interactive.LootItem>();
        }

        public void Start () {
            //
        }

        public void Update() {
            //
        }

        public void OnGUI() {
            if(SPTLootFetchingPlugin.ESPEnable?.Value!=true){return;}
            if(this.LootItem==null){return;}
            if(Camera.main==null){return;}
            Single distance = Vector3.Distance(Camera.main.gameObject.transform.position,this.gameObject.transform.position);
            Single limit = SPTLootFetchingPlugin.Distance?.Value ?? 256F;
            if(limit<=0 || distance>limit){return;}
            Vector3 position = Camera.main.WorldToViewportPoint(this.gameObject.transform.position);
            if(position.x>1F || position.x<0F || position.y>1F || position.y<0F || position.z<0F){return;}
            Vector2 positionUI = new Vector2(){
                x = position.x * Screen.width,
                y = (1F-position.y) * Screen.height
            };
            String name = String.IsNullOrWhiteSpace(this.LootItem.Name.Localized()) ? this.LootItem.Name : this.LootItem.Name.Localized();
            GUI.Label(new Rect(positionUI.x-64F,positionUI.y-12F,128F,24F),String.Concat("<",name,">"),LootNameLabelComponent.GUIStyle);
        }

        public void OnDestroy(){
            this.LootItem = null;
        }
    }
}
