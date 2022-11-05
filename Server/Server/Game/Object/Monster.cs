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
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {

        }
    }
}
