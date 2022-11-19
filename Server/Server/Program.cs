using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.Game;
using ServerCore;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();

		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();
		static void TickRoom(GameRoom room, int tick = 1000)
        {
			var timer = new System.Timers.Timer();
			// 시간 간격
			timer.Interval = tick;
			// 실행 대상
			timer.Elapsed += ((s, e) => { room.Update(); });
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer);
        }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			var d = DataManager.StatDict;

			// 게임룸 생성
			GameRoom room = RoomManager.Instance.Add(1);
			// 자동 업데이트 실행
			TickRoom(room, 50);

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            //FlushRoom();
            //JobTimer.Instance.Push(FlushRoom);

            // 일단 무식하게 무한루프로 Update 검사
            while (true)
			{
				//JobTimer.Instance.Flush();
				Thread.Sleep(100);
			}
		}
	}
}
