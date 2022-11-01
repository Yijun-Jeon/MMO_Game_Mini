using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
                return;

            lock(_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;

                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.info;
                    newPlayer.Session.Send(enterPacket);

                    // 다른 유저들 정보
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach(Player p in _players)
                    {
                        if (newPlayer != p)
                            spawnPacket.Players.Add(p.info);
                    }
                    newPlayer.Session.Send(spawnPacket);
                }

                // 타인한테 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.info);
                    foreach(Player p in _players)
                    {
                        if (newPlayer != p)
                            p.Session.Send(spawnPacket);
                    }
                }
            }   

        }
        public void LeaveGame(int playerId)
        {
            lock(_lock)
            {
                Player player = _players.Find(p => p.info.PlayerId == playerId);
                if (player == null)
                    return;

                _players.Remove(player);
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
                // 타인한테 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.info.PlayerId);
                    foreach(Player p in _players)
                    {
                        if (p != player)
                            p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock(_lock)
            {
                foreach(Player p in _players)
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
                // 일단 서버에서 좌표 이동
                PlayerInfo info = player.info;
                info.PosInfo = movePacket.PosInfo;

                // 다른 플레이어한테도 알려줌
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.info.PlayerId;
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
                PlayerInfo info = player.info;
                // 스킬을 쓸 수 없는 상태
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                // TODO : 스킬 사용 가능 여부 체크 - 쿨타임 등

                // 통과
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.PlayerId = player.info.PlayerId;
                skill.Info.SkillId = 1;
                Broadcast(skill);

                // TODO : 데미지 판정
            }
        }
    }
}
