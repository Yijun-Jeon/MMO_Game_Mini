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
       
    }
}
