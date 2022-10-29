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
    }
}
