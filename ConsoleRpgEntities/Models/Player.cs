using ConsoleRpgEntities.Models.Containers;
using ConsoleRpgEntities.Models.Items;

namespace ConsoleRpgEntities.Models;

public class Player : Character
{
    // Single random source for skill rolls. Static so all players share rolls in-process.
    private static readonly Random _rng = new();

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

    // -------------------------------------------------------------------------
    // W13 — Chest & Monster Loot interaction
    // All operations mutate state in memory; caller is responsible for SaveChanges().
    // -------------------------------------------------------------------------

    // Try to open a chest. Splits responsibility from unlock:
    //  - if already open: AlreadyOpen
    //  - if trapped (and not disarmed): fires the trap, marks disarmed, returns Trapped
    //  - if locked: returns Locked (caller offers unlock/pick)
    //  - else: returns Opened
    public OpenResult OpenChest(Chest chest)
    {
        if (chest is null) throw new ArgumentNullException(nameof(chest));

        if (!chest.IsLocked && (chest.IsTrapped && !chest.TrapDisarmed))
        {
            // Trap fires once.
            chest.TrapDisarmed = true;
            if (Resources is not null)
                Resources.Hp = Math.Max(0, Resources.Hp - chest.TrapDamage);
            return OpenResult.Trapped;
        }

        if (chest.IsLocked)
            return OpenResult.Locked;

        return OpenResult.Opened;
    }

    // Try to unlock a chest using a KeyItem from inventory.
    //   Lockpick (IsKeyItem && KeyId == null):
    //       - chest must be IsPickable AND have no RequiredKeyId
    //       - rolls Reflexes + Lockpicking proficiency vs. UnlockDC (LucentForge integration)
    //       - lockpick is consumed regardless of outcome
    //   Specific key (IsKeyItem && KeyId != null):
    //       - works only if KeyId matches chest.RequiredKeyId
    //       - key is NOT consumed
    public bool TryUnlock(Chest chest, Item key)
    {
        if (chest is null) throw new ArgumentNullException(nameof(chest));
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (!key.IsKeyItem) return false;
        if (!chest.IsLocked) return true;

        if (key.KeyId is null)
        {
            // Lockpick branch.
            if (!chest.IsPickable || chest.RequiredKeyId is not null)
            {
                Inventory?.RemoveItem(key);
                return false;
            }

            int reflexes = Stats?.Reflexes ?? 0;
            int proficiency = CharacterSkills
                .FirstOrDefault(cs => cs.Skill?.Name == "Lockpicking")?.Proficiency ?? 0;
            int roll = _rng.Next(1, 21); // d20
            int total = roll + reflexes + proficiency;

            Inventory?.RemoveItem(key); // consumed regardless
            if (total >= chest.UnlockDC)
            {
                chest.IsLocked = false;
                return true;
            }
            return false;
        }

        // Specific key branch.
        if (chest.RequiredKeyId is null) return false;
        if (string.Equals(key.KeyId, chest.RequiredKeyId, StringComparison.Ordinal))
        {
            chest.IsLocked = false;
            return true;
        }
        return false;
    }

    // W13 graded Task B. Lockpicks (IsKeyItem && KeyId == null) only.
    // Trapped chests only. On success: TrapDisarmed = true, lockpick consumed.
    public bool DisarmTrap(Chest chest, Item lockpick)
    {
        if (chest is null) throw new ArgumentNullException(nameof(chest));
        if (lockpick is null) throw new ArgumentNullException(nameof(lockpick));
        if (!lockpick.IsKeyItem || lockpick.KeyId is not null) return false;
        if (!chest.IsTrapped || chest.TrapDisarmed) return false;

        chest.TrapDisarmed = true;
        Inventory?.RemoveItem(lockpick);
        return true;
    }

    // Move every item from a chest into the player's inventory (if it fits by weight).
    // Items that don't fit stay in the chest.
    public int LootChest(Chest chest)
    {
        if (chest is null) throw new ArgumentNullException(nameof(chest));
        if (Inventory is null) return 0;
        if (chest.IsLocked) return 0;

        int moved = 0;
        var items = chest.ItemsCollection.ToList();
        foreach (var item in items)
        {
            if (!Inventory.CanFit(item.Weight)) continue;
            chest.RemoveItem(item);
            Inventory.AddItem(item);
            moved++;
        }
        return moved;
    }

    // Loot a defeated monster's MonsterLoot container. No-op if already looted.
    public int LootMonster(Npc monster)
    {
        if (monster is null) throw new ArgumentNullException(nameof(monster));
        if (Inventory is null) return 0;
        if (monster.Loot is null || monster.Loot.IsLooted) return 0;

        int moved = 0;
        var items = monster.Loot.ItemsCollection.ToList();
        foreach (var item in items)
        {
            if (!Inventory.CanFit(item.Weight)) continue;
            monster.Loot.RemoveItem(item);
            Inventory.AddItem(item);
            moved++;
        }
        monster.Loot.IsLooted = true;
        return moved;
    }
}
