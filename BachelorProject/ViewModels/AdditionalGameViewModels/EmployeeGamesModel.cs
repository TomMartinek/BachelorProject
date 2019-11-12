using BachelorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.AdditionalGameViewModels
{
    public class EmployeeGamesModel
    {

        public Employee Employee { get; set; }

        public List<GameType> GameTypes { get; set; }

        public List<SummedGame> SummedDuoGames { get; set; }

        public List<SummedGame> SummedSoloGames { get; set; }

        public int TotalCommision { get; set; }

        //public Dictionary<int, int> SummedDuoGames { get; set; }

        //public Dictionary<int, int> SummedSoloGames { get; set; }

    }

    public class SimplifiedGame
    {
        public SimplifiedGame(AdditionalGame additionalGame)
        {
            GameTypeId = additionalGame.GameTypeId;
            Count = additionalGame.Count;
        }

        public int GameTypeId { get; set; }

        public int Count { get; set; }

    }

    public class SummedGame
    {
        public SummedGame(int gameTypeId, int commision, int count)
        {
            GameTypeId = gameTypeId;
            Commision = commision;
            Count = count;
        }


        public int GameTypeId { get; set; }

        public int Commision { get; set; }

        public int Count { get; set; }

    }

}
