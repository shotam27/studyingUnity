using System.Collections.Generic;
using UnityEngine;

// Simple A* pathfinding and reachable-tile calculation for tile-based SRPG.
// Usage example:
//    // Provide helpers depending on your tile system. Example assumes a TileMap with Vector2Int coords:
//    System.Func<Vector2Int, bool> isWalkable = (pos) => { var t = TileMap.GetTileAt(pos); return t != null && !t.IsBlocked; };
//    System.Func<Vector2Int, IEnumerable<Vector2Int>> neighbors = (pos) => {
//        yield return pos + Vector2Int.up;
//        yield return pos + Vector2Int.down;
//        yield return pos + Vector2Int.left;
//        yield return pos + Vector2Int.right;
//    };
//    var path = Pathfinding.FindPath(start, goal, isWalkable, neighbors);
//    var reachable = Pathfinding.GetReachableTiles(start, movePoints, isWalkable, neighbors);
// - Tiles are identified by Vector2Int coordinates (x,y).
// - Caller provides a function to enumerate neighbors and to check if a tile is walkable/occupied.
// - Public API:
//   List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, System.Func<Vector2Int, bool> isWalkable, System.Func<Vector2Int, IEnumerable<Vector2Int>> getNeighbors)
//   HashSet<Vector2Int> GetReachableTiles(Vector2Int start, int movePoints, System.Func<Vector2Int, bool> isWalkable, System.Func<Vector2Int, IEnumerable<Vector2Int>> getNeighbors)

public static class Pathfinding
{
    private class Node
    {
        public Vector2Int Pos;
        public int G;
        public int H;
        public int F => G + H;
        public Node Parent;
    }

    // Manhattan distance heuristic (suitable for 4-way grid)
    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // Reconstruct path from goal node
    private static List<Vector2Int> ReconstructPath(Node node)
    {
        var path = new List<Vector2Int>();
        var n = node;
        while (n != null)
        {
            path.Add(n.Pos);
            n = n.Parent;
        }
        path.Reverse();
        return path;
    }

    // Public: Find path using A*.
    // - isWalkable: returns true if the tile at position can be entered (ignores start).
    // - getNeighbors: returns neighbor tile positions (e.g., 4-way or 8-way).
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, System.Func<Vector2Int, bool> isWalkable, System.Func<Vector2Int, IEnumerable<Vector2Int>> getNeighbors)
    {
        if (start == goal) return new List<Vector2Int> { start };

        var open = new Dictionary<Vector2Int, Node>();
        var closed = new HashSet<Vector2Int>();

        var startNode = new Node { Pos = start, G = 0, H = Heuristic(start, goal), Parent = null };
        open[start] = startNode;

        while (open.Count > 0)
        {
            // pick node with lowest F
            Node current = null;
            foreach (var kv in open)
            {
                if (current == null || kv.Value.F < current.F || (kv.Value.F == current.F && kv.Value.H < current.H))
                {
                    current = kv.Value;
                }
            }

            if (current.Pos == goal)
            {
                return ReconstructPath(current);
            }

            open.Remove(current.Pos);
            closed.Add(current.Pos);

            foreach (var nb in getNeighbors(current.Pos))
            {
                if (closed.Contains(nb)) continue;
                // allow stepping onto goal even if isWalkable false? we assume caller marks goal walkable when appropriate
                if (nb != goal && !isWalkable(nb)) continue;

                int tentativeG = current.G + 1; // assume cost 1 per step; can be extended

                if (!open.TryGetValue(nb, out var neighborNode))
                {
                    neighborNode = new Node { Pos = nb };
                    open[nb] = neighborNode;
                }

                if (tentativeG < neighborNode.G || neighborNode.Parent == null)
                {
                    neighborNode.G = tentativeG;
                    neighborNode.H = Heuristic(nb, goal);
                    neighborNode.Parent = current;
                }
            }
        }

        // no path
        return null;
    }

    // Public: Get reachable tiles within movePoints using breadth-first expansion (weighted by cost=1 per tile)
    // - isWalkable: returns true if tile can be entered
    // - getNeighbors: returns neighbor tiles
    // Returns a set of reachable tile positions (including start)
    public static HashSet<Vector2Int> GetReachableTiles(Vector2Int start, int movePoints, System.Func<Vector2Int, bool> isWalkable, System.Func<Vector2Int, IEnumerable<Vector2Int>> getNeighbors)
    {
        var result = new HashSet<Vector2Int>();
        var frontier = new Queue<(Vector2Int pos, int cost)>();
        var bestCost = new Dictionary<Vector2Int, int>();

        frontier.Enqueue((start, 0));
        bestCost[start] = 0;
        result.Add(start);

        while (frontier.Count > 0)
        {
            var item = frontier.Dequeue();
            var pos = item.pos;
            var cost = item.cost;

            foreach (var nb in getNeighbors(pos))
            {
                int newCost = cost + 1;
                if (newCost > movePoints) continue;
                if (!isWalkable(nb) && newCost != 0) continue; // cannot enter

                if (!bestCost.TryGetValue(nb, out var existing) || newCost < existing)
                {
                    bestCost[nb] = newCost;
                    result.Add(nb);
                    frontier.Enqueue((nb, newCost));
                }
            }
        }

        return result;
    }
}
