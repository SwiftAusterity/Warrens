using NetMud.Authentication;
using System;
using System.Collections.Generic;

namespace NetMud.Models.Features
{
    public class LeaderboardViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public IEnumerable<Tuple<string, double>> DistanceBoard { get; set; }
        public IEnumerable<Tuple<string, double>> HealthBoard { get; set; }

        public LeaderboardViewModel(IEnumerable<Tuple<string, double>> distance, IEnumerable<Tuple<string, double>> health)
        {
            DistanceBoard = distance;
            HealthBoard = health;
        }
    }
}