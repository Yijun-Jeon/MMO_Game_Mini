using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Arrow : Projecttile
    {
        // 화살을 쏜 주인
        public GameObject Owner { get; set; }

        public void Update()
        {

        }
    }
}
