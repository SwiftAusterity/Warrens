using NetMud.DataStructure.Game;
using NetMud.DataStructure.Player;
using System;

namespace NetMud.Data.Game
{
    [Serializable]
    public class Turn : ITurn
    {
        public int Number { get; set; }

        public IPlayer Taker { get; set; }

        public DateTime Start { get; set; }

        public DateTime TrueEnd { get; set; }

        public DateTime ScheduledEnd { get; set; }

        public string Move { get; set; }

        public Turn()
        {
            Number = 0;
            Start = DateTime.Now;
            ScheduledEnd = DateTime.Now.AddMinutes(5);
        }

        public Turn(int turnDuration)
        {
            Number = 0;
            Start = DateTime.Now;
            ScheduledEnd = DateTime.Now.AddMinutes(turnDuration);
        }
    }
}
