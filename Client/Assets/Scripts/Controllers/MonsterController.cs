using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    // ���� Patrol�� ���� �ڷ�ƾ
    Coroutine _coPatrol;
    // Search�� ���� �ڷ�ƾ
    Coroutine _coSearch;
    // ��ǥ ����
    [SerializeField]
    Vector3Int _destCellPos;

    // Search ���
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
            
            // �ڷ�ƾ ����
            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            // �ڷ�ƾ ����
            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void Init()
    {
        // ���� �߿� - animator ���� ã����� �ϱ� ����
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
        // Search ���¶��
        if(_target != null)
        {
            destPos = _target.GetComponent<CreatureController>().CellPos;
        }

        // a* algorithm
        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);

        // ���� ��ã�� ��� || Player�� �ʹ� �־��� ���
        if(path.Count < 2 || (_target != null && path.Count > 10))
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        // ã�� ���� �ϳ��� ������ �ɾ
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

    // �ǰ� ó��
    public override void OnDamaged()
    {
        // �ǰ� ����Ʈ 
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f); // 0.5�� �� �Ҹ�

        // �ǰ�
        Managers.Object.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoPatrol()
    {
        // 1~3�� ���
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);
        
        for(int i=0;i<10;i++)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange);

            // �� �� �ִ� ���̸�
            if(Managers.Map.CanGo(randPos) && Managers.Object.Find(randPos) == null)
            {
                _destCellPos = randPos;
                State = CreatureState.Moving;
                // �ڷ�ƾ Ż��
                yield break;
            }
        }
        // �ڷ�ƾ ����
        State = CreatureState.Idle;
    }

    IEnumerator CoSearch()
    {
        while(true)
        {
            // 1�ʸ��� ��ĵ - ���ϸ� ����
            yield return new WaitForSeconds(1);

            // �̹� Ÿ���� ã�Ҵٸ�
            if (_target != null)
                continue;

            _target = Managers.Object.Find((go) =>
            {
                PlayerController pc = go.GetComponent<PlayerController>();
                // Player�� �ƴ϶��
                if (pc == null)
                    return false;

                Vector3Int dir = pc.CellPos - CellPos;
                // Ž�� �������� �ָ� �ִٸ�
                if (dir.magnitude > _searchRange)
                    return false;

                return true;
            });
        }
    }
}
