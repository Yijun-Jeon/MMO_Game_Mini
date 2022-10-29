using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class PlayerManager
    {
        public static PlayerManager Instance { get; } = new PlayerManager();

        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        int _playerId = 1; // TODO

        // Player 생성
        public Player Add()
        {
            Player player = new Player();
            lock (_lock)
            {
                player.info.PlayerId = _playerId;
                _players.Add(_playerId, player);
                _playerId++;
            }
            return player;
        }

        // Player 제거
        public bool ReMove(int playerId)
        {
            lock (_lock)
            {
                return _players.Remove(playerId);
            }
        }

        // Player 찾음
        public Player Find(int playerId)
        {
            lock (_lock)
            {
                Player player = null;
                if (_players.TryGetValue(playerId, out player))
                    return player;
                return null;
            }
        }
    }
}
