using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;

namespace Server
{
	public class ClientSession : PacketSession
	{
		// 관리하는 내 플레이어
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		public void Send(IMessage packet)
		{
			// packet id 추출
			string msgName = packet.Descriptor.Name.Replace("_", String.Empty); // '_' 제거한 이름 추출
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName); // 이름이 같은 enum 값을 반환

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			// PROTO Test
			MyPlayer = ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.info.Name = $"Player_{MyPlayer.info.ObjectId}";
				MyPlayer.info.PosInfo.State = CreatureState.Idle;
				MyPlayer.info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.info.PosInfo.PosX = 0;
				MyPlayer.info.PosInfo.PosY = 0;
				MyPlayer.Session = this;
			}

			// 1번방에 플레이어 입장
			RoomManager.Instance.Find(1).EnterGame(MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			RoomManager.Instance.Find(1).LeaveGame(MyPlayer.info.ObjectId);

			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
