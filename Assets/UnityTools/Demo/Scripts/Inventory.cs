using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityTools;
namespace UnityToolsDemo {

    [System.Serializable] public class InventoryState : ObjectAttachmentState {
        public Item[] items;
        public InventoryState (Inventory instance) { 
            this.items = instance.items;
        }
    }
    [System.Serializable] public class Item {
        public string name;
        public int amount;
    }


    // on disable: unequip equipped things, and send them to pool

    public class Inventory : MonoBehaviour, IObjectAttachment {
        public int InitializationPhase () {
            return 0;
        }

        public void InitializeDefault () {
            // build inventory with template...
        }

        public void Strip () {
            // remove all inventory
        }


        public ObjectAttachmentState GetState () {
            return new InventoryState (this);
        }

        public void LoadState (ObjectAttachmentState state) {
            InventoryState inventoryState = state as InventoryState; 
            items = inventoryState.items;
        }

        public Item[] items;
    }
}
