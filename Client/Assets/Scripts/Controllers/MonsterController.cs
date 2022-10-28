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
    // Skill을 위한 코루틴
    Coroutine _coSkill;
    // 목표 지점
    [SerializeField]
    Vector3Int _destCellPos;

    // Search 대상
    [SerializeField]
    GameObject _target;
    [SerializeField]
    float _searchRange = 10.0f;
    [SerializeField]
    float _skillRange = 1.0f;

    [SerializeField]
    bool _rangedSkill = false;

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
        _rangedSkill = (Random.Range(0, 2) == 0 ? true : false);

        if (_rangedSkill)
            _skillRange = 10.0f;
        else
            _skillRange = 1.0f;
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

            Vector3Int dir = destPos - CellPos;
            // 스킬 범위에 있고 일직선상일때
            if(dir.magnitude <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                // Player의 위치를 바라보도록 처리
                Dir = GetDirFromVec(dir);

                State = CreatureState.Skill;

                if(_rangedSkill)
                    _coSkill = StartCoroutine("CoStartShootArrow");
                else
                    _coSkill = StartCoroutine("CoStartPunch");
                return;
            }
        }

        // a* algorithm
        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);

        // 길을 못찾은 경우 || Player가 너무 멀어진 경우
        if(path.Count < 2 || (_target != null && path.Count > 20))
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        // 찾은 길을 하나씩 꺼내서 걸어감
        Vector3Int nextPos = path[1];

        Vector3Int moveCellDir = nextPos - CellPos;
        Dir = GetDirFromVec(moveCellDir);

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
            yield return new WaitForSeconds(0.2f);

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
    IEnumerator CoStartPunch()
    {
        // 피격 판정 - 평타가 나오는 즉시
        GameObject go = Managers.Object.Find(GetFrontCellPos());
        if (go != null)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc != null)
                cc.OnDamaged();
        }

        // 0.5초 뒤 자동으로 Moving State로 돌아감
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Moving;
        _coSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController ac = go.GetComponent<ArrowController>();
        // 플레이어가 보고 있는 방향
        ac.Dir = _lastDir;
        // 플레이어의 위치
        ac.CellPos = CellPos;

        // 대기 시간
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Moving;
        _coSkill = null;
    }
}
