using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    protected override void Init()
    {
        // 순서 중요 - animator 먼저 찾아줘야 하기 때문
        base.Init();
        State = CreatureState.Idle;
        Dir = MoveDir.None;
    }
    protected override void UpdateController()
    {
        // GetDirInput();
        base.UpdateController();
    }
    // 이동 키 입력 받음
    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // 기기마다 Frame이 다를 수 있기 때문에 deltaTime을 곱해서 모든 머신에서 같게 보이게 함
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
