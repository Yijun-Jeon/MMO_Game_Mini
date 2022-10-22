using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

// 개발하는 단계에서만 생성되게 설정
#if UNITY_EDITOR
using UnityEditor;
#endif


public class MapEditor
{
#if UNITY_EDITOR

    // Unity 상의 메뉴에 표시
    // 기존 메뉴 산하도 가능
    // 단축키 % (Ctrl), # (Shift), & (Alt)
    [MenuItem("Tools/GenerateMap %#g")]
    private static void GenerateMap()
    {
        // Resource 산하의 모든 게임 오브젝트
        GameObject[] gameObjects =  Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach(GameObject go in gameObjects)
        {
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            // 파일 포맷 - bianry or text
            using (var writer = File.CreateText($"Assets/Resources/Map/{go.name}.txt"))
            {
                writer.WriteLine(tm.cellBounds.xMin);
                writer.WriteLine(tm.cellBounds.xMax);
                writer.WriteLine(tm.cellBounds.yMin);
                writer.WriteLine(tm.cellBounds.yMax);

                for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
                {
                    for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            writer.Write("1");
                        }
                        else
                            writer.Write("0");
                    }
                    writer.WriteLine();
                }
            }
        }
    }

#endif
}
