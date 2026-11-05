using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A track: a grid of road and wall with a start cell, the heading the robot starts
// with, and finish cells. Drawn from text, one character per cell: '#' wall,
// '.' road, 'S' start, 'F' finish, 'O' oil (the real track's difference: the
// robot slides one cell further). Every track knows how many road cells each
// cell is from the finish, so "toward" and "away" can earn or lose points.
public class Track
{
    public string id, name, note;
    public string[] rows;
    public Vector2Int start;
    public int startHeading;   // 0 north, 1 east, 2 south, 3 west
    public int[,] toFinish;    // road cells to the nearest finish; -1 off the road

    public int W => rows[0].Length;
    public int H => rows.Length;
    public bool InBounds(Vector2Int p) => p.x >= 0 && p.y >= 0 && p.x < W && p.y < H;
    public char At(Vector2Int p) => InBounds(p) ? rows[p.y][p.x] : '#';
    public bool Road(Vector2Int p) => At(p) != '#';
    public bool Finish(Vector2Int p) => At(p) == 'F';
    public bool Oil(Vector2Int p) => At(p) == 'O';
    public int Length => toFinish[start.y, start.x];
    public int Distance(Vector2Int p) => InBounds(p) ? toFinish[p.y, p.x] : -1;

    public static Vector2Int Dir(int heading) => heading switch { 0 => new Vector2Int(0, -1), 1 => new Vector2Int(1, 0), 2 => new Vector2Int(0, 1), _ => new Vector2Int(-1, 0) };

    static Track Make(string id, string name, string note, int startHeading, params string[] rows)
    {
        var t = new Track { id = id, name = name, note = note, rows = rows, startHeading = startHeading };
        for (int y = 0; y < t.H; y++) for (int x = 0; x < t.W; x++) if (rows[y][x] == 'S') t.start = new Vector2Int(x, y);
        t.toFinish = new int[t.H, t.W];
        for (int y = 0; y < t.H; y++) for (int x = 0; x < t.W; x++) t.toFinish[y, x] = -1;
        var queue = new Queue<Vector2Int>();
        for (int y = 0; y < t.H; y++) for (int x = 0; x < t.W; x++) if (rows[y][x] == 'F') { t.toFinish[y, x] = 0; queue.Enqueue(new Vector2Int(x, y)); }
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            for (int h = 0; h < 4; h++)
            {
                var n = p + Dir(h);
                if (!t.Road(n) || t.toFinish[n.y, n.x] >= 0) continue;
                t.toFinish[n.y, n.x] = t.toFinish[p.y, p.x] + 1;
                queue.Enqueue(n);
            }
        }
        return t;
    }

    // The tracks, each made for the levels that use it.
    public static readonly Track[] All =
    {
        Make("bend", "One bend", "A straight, one right bend, the finish.", 1,
            "########",
            "#S.....#",
            "######.#",
            "######.#",
            "######F#",
            "########"),
        Make("twobends", "Two bends", "A right bend, a left one, then a right one to the finish.", 1,
            "##########",
            "#S....####",
            "#####.####",
            "#####....#",
            "########.#",
            "########F#",
            "##########"),
        Make("bothways", "Both ways", "A left bend, then a right one. They look the same from the front.", 1,
            "#########",
            "####..F##",
            "####.####",
            "####.####",
            "#S...####",
            "#########"),
        Make("chicane", "The chicane", "After the bend there is one clear cell before the next. Full speed cannot make it.", 1,
            "##########",
            "#S....####",
            "#####.####",
            "#####..###",
            "######.###",
            "######F###",
            "##########"),
        Make("plaza", "The plaza", "An open square on the way: room to drive in circles, if circling pays.", 1,
            "##############",
            "#S...........#",
            "#......#####.#",
            "#......#####.#",
            "#......#####.#",
            "#......#####.#",
            "############F#",
            "##############"),
        Make("hairpin", "The hairpin", "A long way round with a tight turn back.", 1,
            "############",
            "#S........##",
            "#########.##",
            "#........F##",
            "#.##########",
            "#.##########",
            "#.##########",
            "############"),
        Make("grand", "The grand circuit", "A plaza, a chicane and the long way home. One feature pays for a loophole.", 1,
            "###############",
            "#S............#",
            "#.....#######.#",
            "#.....#######.#",
            "#.....######..#",
            "#.....######.##",
            "############.##",
            "########F....##",
            "###############"),
        Make("leftonly", "Left turns only", "Two left bends, nothing else. A robot trained here has never met a right bend.", 3,
            "##########",
            "#.......S#",
            "#.########",
            "#.########",
            "#....F####",
            "##########"),
        Make("rightbend", "The test bend", "One right bend. Never trained on.", 1,
            "#########",
            "#S.....##",
            "######.##",
            "######.##",
            "######F##",
            "#########"),
        Make("real", "The real track", "Like the test bend, with oil on the straight before it: the simulator never had oil.", 1,
            "###########",
            "#S...OO..##",
            "########.##",
            "########.##",
            "########F##",
            "###########"),
    };

    public static Track Get(string id) => All.FirstOrDefault(t => t.id == id) ?? All[0];
}
