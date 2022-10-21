using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestCollision : MonoBehaviour
{
    public Tilemap _tilemap;
    public TileBase _tile;

    void Start()
    {
        _tilemap.SetTile(new Vector3Int(0, 0, 0), _tile);
    }

    void Update()
    {
        List<Vector3Int> blocked = new List<Vector3Int>(); // 갈 수 없는 영역

        // 해당 tilemap의 모든 영역을 스캔
        foreach(Vector3Int pos in _tilemap.cellBounds.allPositionsWithin)
        {
            // Tile이 깔려 있으면 추출하여 List에 추가
            TileBase tile = _tilemap.GetTile(pos);
            if (tile != null)
                blocked.Add(pos);
        }
    }
}
