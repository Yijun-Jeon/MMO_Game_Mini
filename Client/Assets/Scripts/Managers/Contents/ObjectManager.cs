using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    // 서버 연동용
    //Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    List<GameObject> _objects = new List<GameObject>();

    public void Add(GameObject go)
    {
        _objects.Add(go);
    }

    public void Remove(GameObject go)
    {
        _objects.Remove(go);
    }
    // 가장 무식하게 충돌을 찾는 방법 - Client Base
    public GameObject Find(Vector3Int cellPos)
    {
        foreach(GameObject obj in _objects)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if (cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }
        return null;
    }
    // Monster Search용 Find
    public GameObject Find(Func<GameObject,bool> condition)
    {
        foreach (GameObject obj in _objects)
        {
            if (condition.Invoke(obj))
                return obj;
        }
        return null;
    }

    public void Clear()
    {
        _objects.Clear();
    }
}
