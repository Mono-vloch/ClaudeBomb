using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The board: a square grid, the items lying on it and where the agents stand.
// Pure state, no Unity objects; BoardView draws it, Simulation changes it.
public class Arena
{
    public const int Size = 10;

    public readonly ItemKind[,] items = new ItemKind[Size, Size];
    public readonly List<AgentState> agents = new List<AgentState>();
    public readonly Brief brief;
    public readonly System.Random rng;

    public int ItemCount { get; private set; }

    public Arena(Brief brief, IList<AgentConfig> configs, long budgetPerAgent, int seed)
    {
        this.brief = brief;
        rng = new System.Random(seed);
        var homes = Homes(configs.Count);
        for (int i = 0; i < configs.Count; i++)
            agents.Add(new AgentState { config = configs[i], index = i, home = homes[i], pos = homes[i], budget = budgetPerAgent });
        SpawnItems(26);
    }

    public static bool InBounds(Vector2Int p) => p.x >= 0 && p.y >= 0 && p.x < Size && p.y < Size;
    public ItemKind ItemAt(Vector2Int p) => InBounds(p) ? items[p.x, p.y] : null;
    public AgentState AgentAt(Vector2Int p) => agents.FirstOrDefault(a => a.alive && a.pos == p);

    public void RemoveItem(Vector2Int p)
    {
        if (items[p.x, p.y] == null) return;
        items[p.x, p.y] = null;
        ItemCount--;
    }

    // Half the new items are safe, half are poison, so a careful agent always has
    // something to eat and a careless one always has something to regret.
    public void SpawnItems(int count)
    {
        var safe = brief.SafeItems.ToList();
        var poison = brief.PoisonItems.ToList();
        var empty = new List<Vector2Int>();
        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
            {
                var p = new Vector2Int(x, y);
                if (items[x, y] == null && AgentAt(p) == null && !agents.Any(a => a.home == p)) empty.Add(p);
            }
        for (int i = 0; i < count && empty.Count > 0; i++)
        {
            int at = rng.Next(empty.Count);
            var pool = (i % 2 == 0 ? safe : poison);
            if (pool.Count == 0) pool = safe.Count > 0 ? safe : poison;
            items[empty[at].x, empty[at].y] = pool[rng.Next(pool.Count)];
            empty.RemoveAt(at);
            ItemCount++;
        }
    }

    // Starting cells spread evenly around the edge, clockwise from the top middle.
    public static Vector2Int[] Homes(int n)
    {
        var ring = new List<Vector2Int>();
        for (int x = 0; x < Size; x++) ring.Add(new Vector2Int(x, 0));
        for (int y = 1; y < Size; y++) ring.Add(new Vector2Int(Size - 1, y));
        for (int x = Size - 2; x >= 0; x--) ring.Add(new Vector2Int(x, Size - 1));
        for (int y = Size - 2; y >= 1; y--) ring.Add(new Vector2Int(0, y));
        int start = Size / 2 - 1;
        var homes = new Vector2Int[n];
        for (int i = 0; i < n; i++)
            homes[i] = ring[(start + (int)Math.Round(i * ring.Count / (double)n)) % ring.Count];
        return homes;
    }

    public static Vector2Int Step(string move) => move switch
    {
        "north" => new Vector2Int(0, -1),
        "south" => new Vector2Int(0, 1),
        "east"  => new Vector2Int(1, 0),
        "west"  => new Vector2Int(-1, 0),
        _ => Vector2Int.zero,
    };
}
