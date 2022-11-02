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
        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(info.PosInfo.PosX, info.PosInfo.PosY);
            }
            set
            {
                info.PosInfo.PosX = value.x;
                info.PosInfo.PosY = value.y;
            }
        }

        public Vector2Int GetFrontCellPos(MoveDir dir)
        {
            Vector2Int cellPos = CellPos;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    break;
            }

            return cellPos;
        }
    }
}
