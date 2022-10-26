using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
        // ���� ����
        switch (_lastDir)
        {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, -180);
                break;
            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
        base.Init();
    }

    // �ʿ� ���� - override��
    protected override void UpdateAnimation()
    {

    }

    // �̵� ������ ������ ��(Idle) ���� ��ǥ �̵�
    protected override void UpdateIdle()
    {
        if (_dir != MoveDir.None)
        {
            Vector3Int destPos = CellPos;
            switch (_dir)
            {
                case MoveDir.Up:
                    destPos += Vector3Int.up;
                    break;
                case MoveDir.Down:
                    destPos += Vector3Int.down;
                    break;
                case MoveDir.Left:
                    destPos += Vector3Int.left;
                    break;
                case MoveDir.Right:
                    destPos += Vector3Int.right;
                    break;
            }
            State = CreatureState.Moving;

            if (Managers.Map.CanGo(destPos))
            {
                GameObject go = Managers.Object.Find(destPos);
                if (go == null)
                {
                    CellPos = destPos;
                }
                else
                {
                    // ������Ʈ�� �浹 �� �Ҹ�
                    Debug.Log(go.name);
                    Managers.Resource.Destroy(gameObject);
                }
            }
            else
            {
                // ȭ�� �Ҹ�
                Managers.Resource.Destroy(gameObject);
            }
        }
    }
}
