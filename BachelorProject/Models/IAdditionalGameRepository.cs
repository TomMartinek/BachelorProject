using BachelorProject.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public interface IAdditionalGameRepository
    {
        AdditionalGame GetAdditionalGame(int id);
        IEnumerable<AdditionalGame> GetAllAdditionalGames();
        AdditionalGame Add(AdditionalGame additionalGame);
        AdditionalGame Update(AdditionalGame additionalGameChanges);
        AdditionalGame Delete(int id);

        IEnumerable<AdditionalGame> GetMonthAdditionalGames(DateTime month);

        IEnumerable<Employee> GetAllEmployees();
        IEnumerable<GameType> GetAllGameTypes();

        IQueryable<AdditionalGame> GetAllAdditionalGamesAsIQueryable(BranchOfficeEnum branchOffice);
        IQueryable<AdditionalGame> GetMonthAdditionalGamesAsIQueryable(DateTime month, BranchOfficeEnum branchOffice);
    }
}
