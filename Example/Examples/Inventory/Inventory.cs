using Microsoft.Xna.Framework;

namespace Example {
    public class Inventory {
        public bool Open { get; set; } = true;
        public InventorySlot[][] Slots { get; }

        public Inventory() {
            
            Slots = new InventorySlot[3][];
            for (int i = 0; i < Slots.Length; i++) {
                Slots[i] = new InventorySlot[9];
                for (int j = 0; j < Slots[i].Length; j++) {
                    Slots[i][j] = new InventorySlot(null, 0);
                }
            }
            
            Slots[0][4] = new InventorySlot("Sword", 1);
            Slots[1][8] = new InventorySlot("Apple", 3);
            Slots[2][2] = new InventorySlot("Stick", 2);
        }
    }

    public class InventorySlot {
        public InventorySlot(string name, int count) {
            ItemName = name;
            ItemCount = count;
        }

        public void SwapWith(InventorySlot other) {
            string tempName = ItemName;
            int tempCount = ItemCount;
            ItemName = other.ItemName;
            ItemCount = other.ItemCount;
            other.ItemName = tempName;
            other.ItemCount = tempCount;
        }

        public string ItemName { get; set; }
        public int ItemCount { get; set; }
    }
}