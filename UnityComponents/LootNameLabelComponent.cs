using System;
using UnityEngine;

namespace SPTLootFetching.UnityComponents {
    public class LootNameLabelComponent : MonoBehaviour {
        public static GUIStyle GUIStyle{get;} = new GUIStyle(){
            normal = {
                textColor = Color.green
            }
        };

        public void Awake(){
            //
        }

        public void Start () {
            //
        }

        public void Update() {
            //
        }

        public void OnGUI() {
            /*
            if(SPTLootFetchingPlugin.Debug?.Value!=true){return;}
            if(this.Corpse==null){return;}
            Vector3 position = Camera.main.WorldToViewportPoint(this.Corpse.transform.position);
            if(position.x>1F || position.x<0F || position.y>1F || position.y<0F || position.z<0F){return;}
            Vector2 positionUI = new Vector2(){
                x = position.x * Screen.width,
                y = (1F-position.y) * Screen.height
            };
            GUI.Label(new Rect(positionUI.x-48F,positionUI.y-12F,96F,24F),String.Concat("<",this.Corpse.name,">"),LootNameLabelComponent.GUIStyle);
            */
        }

        public void OnDestroy(){
            //
        }
    }
}
