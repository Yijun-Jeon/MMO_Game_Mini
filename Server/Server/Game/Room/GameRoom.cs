﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int,Player> _players = new Dictionary<int,Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int,Monster>();
        Dictionary<int, Projecttile> _projecttiles = new Dictionary<int,Projecttile>();

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId,"../../../../../Common/MapData");
        }

        public void Update()
        {
            lock(_lock)
            {
                foreach(Projecttile projecttile in _projecttiles.Values)
                {
                    projecttile.Update();
                }
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            lock(_lock)
            {
                if(type == GameObjectType.Player)
                {
                    Player player = gameObject as Player;
                    _players.Add(gameObject.Id, player);
                    player.Room = this;

                    // 본인한테 정보 전송
                    {
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = player.info;
                        player.Session.Send(enterPacket);

                        // 다른 유저들 정보
                        S_Spawn spawnPacket = new S_Spawn();
                        foreach (Player p in _players.Values)
                        {
                            if (player != p)
                                spawnPacket.Objects.Add(p.info);
                        }
                        player.Session.Send(spawnPacket);
                    }
                }
                else if(type == GameObjectType.Monster)
                {
                    Monster monster = gameObject as Monster;
                    _monsters.Add(monster.Id, monster);
                    monster.Room = this;

                }
                else if(type == GameObjectType.Projecttile)
                {
                    Projecttile projecttile = gameObject as Projecttile;
                    _projecttiles.Add(projecttile.Id, projecttile);
                    projecttile.Room = this;
                }
               

                // 타인한테 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(gameObject.info);
                    foreach(Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPacket);
                    }
                }
            }   

        }
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.Remove(objectId, out player) == false)
                        return;

                    player.Room = null;
                    Map.ApplyLeave(player);

                    // 본인한테 정보 전송
                    {
                        S_LeaveGame leavePacket = new S_LeaveGame();
                        player.Session.Send(leavePacket);
                    }
                }
                else if (type == GameObjectType.Monster)
                {
                    Monster monster = null;
                    if (_monsters.Remove(objectId, out monster) == false)
                        return;

                    monster.Room = null;
                    Map.ApplyLeave(monster);
                }
                else if (type == GameObjectType.Projecttile)
                {
                    Projecttile projecttile = null;
                    if (_projecttiles.Remove(objectId, out projecttile) == false)
                        return;

                    projecttile.Room = null;
                }

                // 타인한테 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectIds.Add(objectId);
                    foreach(Player p in _players.Values)
                    {
                        if (p.Id != objectId)
                            p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock(_lock)
            {
                foreach(Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // 실질적인 정보 수정은 한 쓰레드만 실행되도록
            lock(_lock)
            {
                PositionInfo movePosInfo = movePacket.PosInfo;
                ObjectInfo info = player.info;

                // 다른 좌표로 이동하려는 경우, 갈 수 있는지 체크
                if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                        return;
                }

                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                // Map의 _players 갱신
                Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                // 다른 플레이어한테도 알려줌
                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectId = player.info.ObjectId;
                resMovePacket.PosInfo = movePacket.PosInfo;

                Broadcast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            lock(_lock)
            {
                ObjectInfo info = player.info;
                // 스킬을 쓸 수 없는 상태
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                // TODO : 스킬 사용 가능 여부 체크 - 쿨타임 등

                // 통과
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = player.info.ObjectId;
                skill.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(skill);

                Data.Skill skillData = null;
                if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                    return;
                switch (skillData.skillType)
                {
                    case SkillType.SkillAuto:
                        {
                            // TODO : 데미지 판정
                            Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                            // 해당 위치에 있는 플레이어 반환
                            GameObject target = Map.Find(skillPos);
                            if (target != null)
                            {
                                Console.WriteLine("Hit GameObject!");
                            }
                        }
                        break;
                    case SkillType.SkillProjecttile:
                        {
                            Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                            if (arrow == null)
                                return;

                            arrow.Owner = player;
                            arrow.Data = skillData;
                            arrow.PosInfo.State = CreatureState.Moving;
                            arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                            arrow.PosInfo.PosX = player.PosInfo.PosX;
                            arrow.PosInfo.PosY = player.PosInfo.PosY;
                            arrow.Speed = skillData.projecttile.speed;
                            // 화살도 서버에서 관리하여 클라로 전송
                            EnterGame(arrow);
                        }
                        break;
                }
            }
        }
    }
}

