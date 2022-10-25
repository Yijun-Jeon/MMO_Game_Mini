using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    protected override void Init()
    {
        // ���� �߿� - animator ���� ã����� �ϱ� ����
        base.Init();
        State = CreatureState.Idle;
        Dir = MoveDir.None;
    }
    protected override void UpdateController()
    {
        // GetDirInput();
        base.UpdateController();
    }
    // �̵� Ű �Է� ����
    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // ��⸶�� Frame�� �ٸ� �� �ֱ� ������ deltaTime�� ���ؼ� ��� �ӽſ��� ���� ���̰� ��
            //transform.position += Vector3.up * Time.deltaTime * _speed;
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            //transform.position += Vector3.down * Time.deltaTime * _speed;
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            //transform.position += Vector3.left * Time.deltaTime * _speed;
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            //transform.position += Vector3.right * Time.deltaTime * _speed;
            Dir = MoveDir.Right;
        }
        else
        {
            Dir = MoveDir.None;
        }
    }
}
