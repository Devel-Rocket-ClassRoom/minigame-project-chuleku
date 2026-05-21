using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : MonoBehaviour
{
    public MonsterSpawnManager Instance {get; private set;}
    private Coroutine firstMonsterCor;
    private Coroutine secondMonsterCor;
    private TileMap tileMap;
    private Vector2Int tileGrid;
    private int alivecount;
    private int allCount;

    void Awake()
    {
        GameObject go = GameObject.FindWithTag("TileMap");
        tileMap = go.GetComponent<TileMap>();
    }
    public void StageStartSpawnMonster(int stage)
    {
        var Groups = DataTableManager.StageTable.Get(stage);
        alivecount = 0;
        foreach(var g in Groups)allCount += g.Count;
        alivecount = allCount;

    }
    private IEnumerator SpawnMonster(float spawntime,int count,float delay,List<Vector2Int> path,GameObject prefab)
    {
        yield return new WaitForSeconds(spawntime);
        float c = 0;
        while(c<count)
        {
            
            Vector3 pos = tileMap.GridToWorld(TileMap.Start);
            GameObject go =Instantiate(prefab,pos,Quaternion.identity);
            go.GetComponent<MoveEnemy>().SetPath(path);
            c++;
            yield return new WaitForSeconds(delay);
        }
    }


}
