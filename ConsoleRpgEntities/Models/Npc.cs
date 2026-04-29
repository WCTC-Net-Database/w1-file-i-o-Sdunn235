using ConsoleRpgEntities.Models.Containers;

namespace ConsoleRpgEntities.Models;

public class Npc : Character
{
    // W13 — optional MonsterLoot container. Empty/null until a loot table seeds one.
    // Nullable so non-monster NPCs (shopkeepers, quest givers) carry no loot.
    public int? LootId { get; set; }
    public virtual MonsterLoot? Loot { get; set; }
}
