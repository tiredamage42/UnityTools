using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityTools;
namespace UnityToolsDemo {

    public class Inventory : MonoBehaviour, ISaveableAttachment {
        [System.Serializable] public class Item {
            public string name;
            public int amount;
        }

        public Item[] items;

        public Type AttachmentType () { return typeof (Inventory); }
        public object OnSaved () {
            return items;
        }
        public void OnLoaded (object savedAttachmentInfo) {
            items = (Item[])savedAttachmentInfo;
        } 
    }
}
