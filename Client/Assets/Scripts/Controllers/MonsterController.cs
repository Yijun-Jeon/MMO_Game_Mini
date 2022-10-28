using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    // 랜덤 Patrol을 위한 코루틴
    Coroutine _coPatrol;
    // Search를 위한 코루틴
    Coroutine _coSearch;
    // 목표 지점
    [SerializeField]
    Vector3Int _destCellPos;

    // Search 대상
    [SerializeField]
    GameObject _target;
    [SerializeField]
    float _searchRange = 5.0f;

    public override CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            base.State = value;
            
            // 코루틴 정지
            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            // 코루틴 정지
            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void Init()
    {
        // 순서 중요 - animator 먼저 찾아줘야 하기 때문
        base.Init();

        State = CreatureState.Idle;
        Dir = MoveDir.None;

        _speed = 3.0f;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if(_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }

        if (_coSearch == null)
        {
            _coSearch = StartCoroutine("CoSearch");
        }
    }

    protected override void MoveToNextPos()
    {
        Vector3Int destPos = _destCellPos;
        // Search 상태라면
        if(_target != null)
        {
            destPos = _target.GetComponent<CreatureController>().CellPos;
        }

        // a* algorithm
        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);

        // 길을 못찾은 경우 || Player가 너무 멀어진 경우
        if(path.Count < 2 || (_target != null && path.Count > 10))
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        // 찾은 길을 하나씩 꺼내서 걸어감
        Vector3Int nextPos = path[1];

        Vector3Int moveCellDir = nextPos - CellPos;
        if (moveCellDir.x > 0)
            Dir = MoveDir.Right;
        else if (moveCellDir.x < 0)
            Dir = MoveDir.Left;
        else if (moveCellDir.y > 0)
            Dir = MoveDir.Up;
        else if (moveCellDir.y < 0)
            Dir = MoveDir.Down;
        else
            Dir = MoveDir.None;

        if (Managers.Map.CanGo(nextPos) && Managers.Object.Find(nextPos) == null)
        {
            CellPos = nextPos;
        }
        else
        {
            State = CreatureState.Idle;
        }

    }

    // 피격 처리
    public override void OnDamaged()
    {
        // 피격 이펙트 
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f); // 0.5초 뒤 소멸

        // 피격
        Managers.Object.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoPatrol()
    {
        // 1~3초 대기
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);
        
        for(int i=0;i<10;i++)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange);

            // 갈 수 있는 곳이면
            if(Managers.Map.CanGo(randPos) && Managers.Object.Find(randPos) == null)
            {
                _destCellPos = randPos;
                State = CreatureState.Moving;
                // 코루틴 탈출
                yield break;
            }
        }
        // 코루틴 정지
        State = CreatureState.Idle;
    }

    IEnumerator CoSearch()
    {
        while(true)
        {
            // 1초마다 스캔 - 부하를 줄임
            yield return new WaitForSeconds(1);

            // 이미 타겟을 찾았다면
            if (_target != null)
                continue;

            _target = Managers.Object.Find((go) =>
            {
                PlayerController pc = go.GetComponent<PlayerController>();
                // Player가 아니라면
                if (pc == null)
                    return false;

                Vector3Int dir = pc.CellPos - CellPos;
                // 탐색 범위보다 멀리 있다면
                if (dir.magnitude > _searchRange)
                    return false;

                return true;
            });
        }
    }
}
