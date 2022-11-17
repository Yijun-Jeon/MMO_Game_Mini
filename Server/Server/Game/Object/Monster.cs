using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public Monster()
        {
            ObjectType = GameObjectType.Monster;

            // TEMP
            Stat.Level = 1;
            Stat.Hp = 100;
            Stat.MaxHp = 100;
            Stat.Speed = 5.0f;

            State = CreatureState.Idle;
        }

        // FSM (Finite State Machine)
        public override void Update()
        {
            switch(State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }
        }

       
        // 일단 무식한 방법으로 초 계산
        long _nextSearchTick = 0;

        // search 범위
        int _searchCellDist = 10;
        Player _target;

        protected virtual void UpdateIdle()
        {
            // 아직 search tick이 되지 않음
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;// 1초마다 통과함

            Player target =  Room.FindPlayer(p =>
            {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist;
            });

            if (target == null)
                return;

            _target = target;
            State = CreatureState.Moving;
        }

        long _nextMoveTick = 0;
        // 추적 범위
        int _chaseCellDist = 20;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            // 타겟이 없는 경우
            // 타겟이 나가거나 다른 지역으로 이동하는 경우
            if(_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            int dist = (_target.CellPos - CellPos).cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            // 플레이어나 몬스터는 충돌 무시
            List<Vector2Int> path =  Room.Map.FindPath(CellPos, _target.CellPos,checkObject: false);
            if(path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            // 이동
            Dir = GetDirFromVec(path[1] - CellPos); // 방향 vector
            Room.Map.ApplyMove(this, path[1]);

            // 플레이어들에게 알려줌
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
        }

        protected virtual void UpdateSkill()
        {

        }

        protected virtual void UpdateDead()
        {

        }

        public override void OnDamaged(GameObject attacker, int damage)
        {

        }
    }
}
