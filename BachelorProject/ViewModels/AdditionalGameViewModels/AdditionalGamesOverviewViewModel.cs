using BachelorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.AdditionalGameViewModels
{
    public class AdditionalGamesOverviewViewModel
    {

        public AdditionalGamesOverviewViewModel()
        {
            EmployeeGames = new List<EmployeeGamesModel>();
        }

        public List<EmployeeGamesModel> EmployeeGames { get; set; }

        public IEnumerable<GameType> GameTypes { get; set; }

        public DateTime SelectedDate { get; set; }
    }
}
