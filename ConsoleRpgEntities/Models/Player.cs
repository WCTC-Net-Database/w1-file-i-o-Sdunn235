using ConsoleRpgEntities.Models.Containers;
using ConsoleRpgEntities.Models.Items;

namespace ConsoleRpgEntities.Models;

public class Player : Character
{
    // --- W12 Inventory verbs ---
    // All operations mutate state in memory; caller is responsible for SaveChanges().

    public bool PickUp(Item item)
    {
        if (Inventory is null) return false;
        if (!Inventory.CanFit(item.Weight)) return false;

        Inventory.AddItem(item);
        return true;
    }

    public void Drop(Item item)
    {
        if (Inventory is not null && Inventory.ItemsCollection.Contains(item))
            Inventory.RemoveItem(item);
        else
            item.ContainerId = null;
    }

    public bool Equip(Item item)
    {
        if (Equipment is null) return false;

        // Pick a compatible slot based on item type.
        var slot = PickSlotFor(item);
        if (slot is null) return false;

        // Move item from Inventory (or wherever) into the Equipment container.
        Inventory?.ItemsCollection.Remove(item);
        Equipment.AddItem(item);
        slot.EquippedItemId = item.Id;
        slot.EquippedItem = item;
        return true;
    }

    public void Unequip(Item item)
    {
        if (Equipment is null) return;

        var slot = Equipment.Slots.FirstOrDefault(s => s.EquippedItemId == item.Id);
        if (slot is not null)
        {
            slot.EquippedItemId = null;
            slot.EquippedItem = null;
        }

        Equipment.ItemsCollection.Remove(item);
        Inventory?.AddItem(item);
    }

    public void UseItem(Consumable item)
    {
        if (Resources is null) return;

        // Minimal effect dispatch — extend as consumable effects grow.
        switch (item.Effect.ToLowerInvariant())
        {
            case "heal":
                Resources.Hp = Math.Min(Resources.MaxHp, Resources.Hp + item.Potency);
                break;
            case "stamina":
                Resources.Sp = Math.Min(Resources.MaxSp, Resources.Sp + item.Potency);
                break;
            case "bp":
                Resources.Bp = Math.Min(Resources.MaxBp, Resources.Bp + item.Potency);
                break;
            case "bytepool":
                Resources.BytePool = Math.Min(Resources.MaxBytePool, Resources.BytePool + item.Potency);
                break;
        }

        // Consumables leave inventory when used.
        Inventory?.RemoveItem(item);
    }

    private EquipmentSlot? PickSlotFor(Item item)
    {
        if (Equipment is null) return null;

        return item switch
        {
            Weapon => Equipment.Slots.FirstOrDefault(s =>
                (s.Slot == Enums.SlotType.MainHand || s.Slot == Enums.SlotType.OffHand)
                && s.EquippedItemId is null),
            Armor armor => Equipment.Slots.FirstOrDefault(s =>
                BodySlotToSlotType(armor.Slot) == s.Slot && s.EquippedItemId is null),
            _ => null
        };
    }

    private static Enums.SlotType BodySlotToSlotType(Enums.BodySlot body) => body switch
    {
        Enums.BodySlot.Head => Enums.SlotType.Head,
        Enums.BodySlot.Chest => Enums.SlotType.Chest,
        Enums.BodySlot.Legs => Enums.SlotType.Legs,
        Enums.BodySlot.Feet => Enums.SlotType.Feet,
        Enums.BodySlot.Hands => Enums.SlotType.Hands,
        _ => Enums.SlotType.Chest
    };
}
