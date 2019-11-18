using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityTools;
namespace UnityToolsDemo {

    // on disable: unequip equipped things, and send them to pool

    public class Inventory : MonoBehaviour, ISaveAttachment {
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
