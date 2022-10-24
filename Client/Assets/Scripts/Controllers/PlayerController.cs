using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    public float _speed = 5.0f;
    Vector3Int _cellPos = new Vector3Int(-4,1,0); // 좌표상에 실제 위치
    bool _isMoving = false;
    Animator _animator;
    MoveDir _dir = MoveDir.Down;
    // 플레이어의 방향을 바꿀 때 바로 애니메이션도 같이 처리
    public MoveDir Dir
    {
        get{return _dir;}
        set
        {
            if(_dir == value)
                return;
            
            switch(value)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    break;
                // Scale 대칭 필요
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    transform.localScale = new Vector3(-1.0f,1.0f,1.0f);
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    break;
                case MoveDir.None:
                    if(_dir == MoveDir.Up)
                    {
                        _animator.Play("IDLE_BACK");
                        transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    }
                    else if(_dir == MoveDir.Down)
                    {
                        _animator.Play("IDLE_FRONT");
                        transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    }
                    else if(_dir == MoveDir.Left)
                    {
                        _animator.Play("IDLE_RIGHT");
                        transform.localScale = new Vector3(-1.0f,1.0f,1.0f);
                    }
                    else
                    {
                        _animator.Play("IDLE_RIGHT");
                        transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    }
                    break;
            }

            _dir = value;
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f,0.6f);
        transform.position = pos;
    }

    void Update()
    {
        GetDirInput();
        UpdatePosition();
        UpdateIsMoving();
    }
    // 카메라 제어의 경우 LateUpdate에서 주로 설정
    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
    // 이동 키 입력 받음
    void GetDirInput()
    {
        if(Input.GetKey(KeyCode.W))
        {
            // 기기마다 Frame이 다를 수 있기 때문에 deltaTime을 곱해서 모든 머신에서 같게 보이게 함
            //transform.position += Vector3.up * Time.deltaTime * _speed;
            Dir = MoveDir.Up;
        }
        else if(Input.GetKey(KeyCode.S))
        {  
            //transform.position += Vector3.down * Time.deltaTime * _speed;
            Dir = MoveDir.Down;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            //transform.position += Vector3.left * Time.deltaTime * _speed;
            Dir = MoveDir.Left;
        }
        else if(Input.GetKey(KeyCode.D))
        {
            //transform.position += Vector3.right * Time.deltaTime * _speed;
            Dir = MoveDir.Right;
        }
        else
        {
            Dir = MoveDir.None;
        }
    }

    // 실제로 스르르 이동
    void UpdatePosition()
    {
        if(_isMoving == false)
            return;

        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f,0.6f);
        // 방향 vector - 2가지의 정보 : 실제 이동하는 방향, 이동하려는 목적지까지의 크기
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if(dist < _speed * Time.deltaTime)
        {
            transform.position = destPos;
            _isMoving = false;
        }
        else
        {
            // 스르르 움직이게 처리
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            _isMoving = true;
        }
    }

    // 이동 가능한 상태일 때 실제 좌표 이동
    void UpdateIsMoving()
    {
        if(_isMoving == false && _dir != MoveDir.None)
        {
            Vector3Int destPos = _cellPos;
            switch(_dir)
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
            
            if(Managers.Map.CanGo(destPos))
            {
                _cellPos = destPos;
                _isMoving = true;
            }
        }
    }
}
