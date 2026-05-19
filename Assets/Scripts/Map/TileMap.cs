using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileMap : MonoBehaviour
{
    public const int W = 20;
    public const int H = 10;

    public static readonly Vector2Int Start = new(0, 0);
    public static readonly Vector2Int Goal = new(W - 1, H - 1);
    public float CellSize { get; private set; }

    Tile[,] tiles;
    Vector3 origin;
    public Vector3 Origin => origin;

    void Awake()
    {
        tiles = new Tile[W, H];

        if (transform.childCount == 0)
        {
            Debug.LogError("TileMap: 자식 타일이 없음");
            return;
        }

        origin = FindOrigin();
        CellSize = DetectCellSize();

        foreach (Transform child in transform)
        {
            var (x, z) = WorldToGrid(child.position);
            if (!IsInBounds(x, z))
            {
                Debug.LogWarning($"TileMap: '{child.name}' 그리드 밖 ({x},{z})");
                continue;
            }
            var tile = child.GetComponent<Tile>() ?? child.gameObject.AddComponent<Tile>();
            tiles[x, z] = tile;
        }
        tiles[0,0].dontBreak = true;
        tiles[W-1,H-1].dontBreak = true;
    }

    Vector3 FindOrigin()
    {
        // 좌상단(시작점) = X 최소 & Z 최대
        Vector3 o = transform.GetChild(0).position;
        foreach (Transform child in transform)
        {
            if (child.position.x < o.x) o.x = child.position.x;
            if (child.position.z > o.z) o.z = child.position.z;
        }
        return o;
    }
    // 마우스 좌클릭 위치를 그리드 좌표로 변환해 로그


    // 직접 배치한 타일들의 최소 x간격을 CellSize로 사용 (CLAUDE.md: ≈3.48)
    float DetectCellSize()
    {
        float best = float.MaxValue;
        var first = transform.GetChild(0).position;
        foreach (Transform c in transform)
        {
            float dx = Mathf.Abs(c.position.x - first.x);
            if (dx > 0.01f && dx < best) best = dx;
        }
        return best == float.MaxValue ? 1f : best;
    }

    public bool IsInBounds(int x, int z) => x >= 0 && x < W && z >= 0 && z < H;

    public Tile GetTile(int x, int z) => IsInBounds(x, z) ? tiles[x, z] : null;

    public bool IsWalkable(int x, int z)
    {
        var t = GetTile(x, z);
        
        return t != null && !t.hasWall;
    }

    public (int x, int z) WorldToGrid(Vector3 pos)
        => (Mathf.RoundToInt((pos.x - origin.x) / CellSize),
            Mathf.RoundToInt((origin.z - pos.z) / CellSize));

    public Vector3 GridToWorld(int x, int z)
        => new Vector3(origin.x + x * CellSize, 0f, origin.z - z * CellSize);

    public Vector3 GridToWorld(Vector2Int g) => GridToWorld(g.x, g.y);
    public void CreateWall(int x, int z,GameObject gm)
    {
        tiles[x,z].hasWall = true;
        tiles[x,z].Wall = gm;
    }
    public void BreakWall(int x,int z)
    {
        if(tiles[x,z].Wall == null)return;
        Destroy(tiles[x,z].Wall);
        tiles[x,z].Wall = null;
        tiles[x,z].hasWall = false;
    }
    public void CreateUnit(int x,int z,GameObject gm)
    {
        if(tiles[x,z].Wall == null||tiles[x,z].Unit !=null) return;
        tiles[x,z].Unit = gm;
    }
    public void BreakUnit(int x,int z)
    {
        if(tiles[x,z].Wall == null||tiles[x,z].Unit ==null) return;
        Destroy(tiles[x,z].Unit);
        tiles[x,z].Unit = null;
    }
    public bool DonCreateCheck(int x, int z)
    {
        return tiles[x,z].dontBreak;
    }
    public bool WallCheck(int x, int z)
    {
        return tiles[x,z].hasWall;
    }
    public bool UnitCheck(int x,int z)
    {
        return tiles[x,z].Unit == null;
    }
    public void AllWallBreak()
    {
        for(int i =0;i<H;i++)
        {
            for(int j=0;j<W;j++)
            {
                if(WallCheck(j,i))
                {
                    BreakWall(j,i);
                }
            }
        }
    }
}
