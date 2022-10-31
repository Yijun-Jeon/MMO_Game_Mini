using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player
    {
        public PlayerInfo info { get; set; } = new PlayerInfo() { 
            PosInfo = new PositionInfo()};
        public GameRoom Room { get; set; }
        public ClientSession Session { get; set; }
    }
}
