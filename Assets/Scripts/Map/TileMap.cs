using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileMap : MonoBehaviour
{
    public const int W = 20;
    public const int H = 10;

    public static readonly Vector2Int Start = new(0, 0);
    public static readonly Vector2Int Goal = new(W - 1, H - 1);
    public float CellSize { get; private set; }
    public Tile[,] TilesView => tiles;
    Tile[,] tiles;
    Vector3 origin;
    public Vector3 Origin => origin;
    private bool pathCheck;
    private Coroutine colorcor;
    private bool firstWallBreakCheck;
    private bool firstCreateWallCheck;
    private bool pathbool;

    void Awake()
    {
        tiles = new Tile[W, H];

        if (transform.childCount == 0)
        {
            Debug.LogError("TileMap: 자식 타일이 없음");
            return;
        }
        if(colorcor!=null) StopCoroutine(colorcor);
        colorcor =null;

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
            if(x==0&&z==0||x==19&&z==9)continue;
            tiles[x,z].origin = tile.GetComponent<Renderer>().material.color; 
        }
        tiles[0,0].dontBreak = true;
        tiles[W-1,H-1].dontBreak = true;
        pathCheck = false;
        firstWallBreakCheck = false;
        firstCreateWallCheck = false;
        pathbool = true;
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
    public void CreateWall(int x, int z,GameObject gm,int stage,int cost)
    {
        tiles[x,z].hasWall = true;
        tiles[x,z].Wall = gm;
        tiles[x,z].wallStageID = stage;
        tiles[x,z].installCost = cost;
        PathColorChange();
        if(firstCreateWallCheck)return;
        SoundManager.Play("CreateWall");
    }
    public void CreateWallCoupon(int x, int z,GameObject gm,int stage,int coupon)
    {
        tiles[x,z].hasWall = true;
        tiles[x,z].Wall = gm;
        tiles[x,z].wallStageID = stage;
        tiles[x,z].Coupon=coupon;
        PathColorChange();
        if(firstCreateWallCheck)return;
        SoundManager.Play("CreateWall");
    }
    public void BreakWall(int x,int z,int stage,int amount)
    {
        if(tiles[x,z].Wall == null)return;

        if (firstWallBreakCheck)
        {
            Destroy(tiles[x,z].Wall);
            tiles[x,z].Wall = null;
            tiles[x,z].hasWall = false;
            tiles[x,z].wallStageID = -1;
            tiles[x,z].Coupon = -1;
            return;
        }
       
        if(tiles[x,z].wallStageID == stage)
        {
            Destroy(tiles[x,z].Wall);
            if(tiles[x,z].Coupon == 1)
            {
                tiles[x,z].Coupon = -1;
                ResourceManager.Instance.AddFreeCreateWallCoupon(1);
                tiles[x,z].Wall = null;
                tiles[x,z].hasWall = false;
                tiles[x,z].wallStageID = -1;
                tiles[x,z].installCost = 0;
                SoundManager.Play("BreakWall");
                PathColorChange();
                return;
            }
            tiles[x,z].Wall = null;
            tiles[x,z].hasWall = false;
            tiles[x,z].wallStageID = -1;
            ResourceManager.Instance.AddGold(tiles[x,z].installCost);
            tiles[x,z].installCost = 0;
            SoundManager.Play("BreakWall");
            PathColorChange();
            return;
        }
        else
        {
            if(ResourceManager.Instance.TrySpendGold(amount))
            {
                Destroy(tiles[x,z].Wall);
                tiles[x,z].Wall = null;
                tiles[x,z].hasWall = false;
                tiles[x,z].wallStageID = -1;
                tiles[x,z].Coupon = -1;
                SoundManager.Play("BreakWall");
                PathColorChange();
            }
            else
            {
                Debug.Log("벽 부수기 골드 부족");
                CenterToast.Show("골드가 부족 합니다.");
                SoundManager.Play("Noop");
            }
        }
        
    }
    public void CreateUnit(int x,int z,GameObject gm)
    {
        if(tiles[x,z].Wall == null||tiles[x,z].Unit !=null) return;
        tiles[x,z].Unit = gm;
        
    }
    public void BreakUnit(int x,int z)
    {
        if(tiles[x,z].Wall == null||tiles[x,z].Unit ==null) return;
        var unitGo = tiles[x,z].Unit;
        // 카드 매니저 측 슬롯 placedUnit/패널 버튼도 동기화
        if (CardGameManager.Instance != null)
            CardGameManager.Instance.NotifyPlacedUnitRemoved(unitGo);
        Destroy(unitGo);
        tiles[x,z].Unit = null;
        PathColorChange();
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
        firstWallBreakCheck = true;
        for(int i =0;i<H;i++)
        {
            for(int j=0;j<W;j++)
            {
                if(WallCheck(j,i))
                {
                    BreakWall(j,i,0,0);
                }
            }
        }
        firstWallBreakCheck =false;
    }
    public void WarningWallColor(int stage)
    {
        if(pathCheck)return;

        if(colorcor!=null) StopCoroutine(colorcor);
        colorcor =null;
        colorcor = StartCoroutine(WarningWallColorcort(stage));
        
    }
    public IEnumerator WarningWallColorcort(int stage)
    {
        pathCheck =true;
        var thisRound = new List<Vector2Int>();
        for (int z = 0; z < H; z++)
            for (int x = 0; x < W; x++)
                if (tiles[x, z] != null && tiles[x, z].hasWall && tiles[x, z].wallStageID == stage)
                    thisRound.Add(new Vector2Int(x, z));

        var culprits = new List<Vector2Int>();
        foreach (var w in thisRound)
        {
            var t = tiles[w.x, w.y];
            t.hasWall = false;
            var path = Pathfinder.FindPath(this, TileMap.Start, TileMap.Goal);
            t.hasWall = true;      
            if (path != null) culprits.Add(w);
        }

        var target = culprits.Count > 0 ? culprits : thisRound;
        
        var original = new List<(Renderer rend, Color c)>();
        foreach(var g in target)
        {
            var wall = tiles[g.x, g.y].Wall;
            if (wall == null) continue;
            var rend = wall.GetComponent<Renderer>();
            if (rend == null) continue;
            original.Add((rend, rend.material.color));
            rend.material.color = Color.red;
        }
        yield return new WaitForSeconds(3f);
        foreach(var (r,c) in original)
        {
            if(r!=null) r.material.color = c;
        }
        pathCheck = false;
    }
    public void FirstCreateWall(bool c)
    {
        firstCreateWallCheck = c;
    }
    public void PathColorChange()
    {
        if(pathbool)return;
        // 완주 시 전체 경로, 막혔으면 갈 수 있는 데까지의 부분 경로를 받는다.
        var path = Pathfinder.FindPathPartial(this, TileMap.Start, TileMap.Goal, out bool reachedGoal);

        // 먼저 모든 타일을 원색으로 되돌린다(막혔을 때 "그 이후" 초록색을 지우는 역할).
        for (int z = 0; z < H; z++)
            for (int x = 0; x < W; x++)
            {
                if((x==0&&z==0)||(x==19&&z==9))continue; // 시작/끝점은 원색 보존 대상에서 제외
                tiles[x,z].GetComponent<Renderer>().material.color = tiles[x,z].origin;
            }

        if(path == null)return; // 시작점 자체가 막힘 등 → 경로 없음

        // 완주면 초록, 막혔으면 노란색으로 "여기까지밖에 못 간다"를 표시.
        Color pathColor = reachedGoal ? Color.green : Color.yellow;

        // 막혔을 때는 부분 경로 끝점만 닿는 데까지 칠해지고 그 이후는 위에서 이미 원색으로 지워짐.
        foreach(var p in path)
        {
            if((p.x==0&&p.y==0)||(p.x==19&&p.y==9))continue; // 시작/끝점은 칠하지 않고 다음 칸으로
            tiles[p.x,p.y].GetComponent<Renderer>().material.color = pathColor;
        }
    }
    public void PathOnOff(bool c)
    {
        pathbool = c;
        if(pathbool)
        {
            for (int z = 0; z < H; z++)
                for (int x = 0; x < W; x++)
                {
                    if((x==0&&z==0)||(x==19&&z==9))continue; // 시작/끝점은 원색 보존 대상에서 제외
                    tiles[x,z].GetComponent<Renderer>().material.color = tiles[x,z].origin;
                }
        }
        else
        {
            PathColorChange();
        }
       
    }
}
