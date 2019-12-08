using BachelorProject.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BachelorProject.Models
{
    // implementace rozhraní IAdditionalGame repository
    public class SQLAdditionalGameRepository : IAdditionalGameRepository
    {
        private readonly AppDbContext context;

        public SQLAdditionalGameRepository(AppDbContext context)
        {
            this.context = context;
        }

        public AdditionalGame Add(AdditionalGame additionalGame)
        {
            context.Add(additionalGame);
            context.SaveChanges();
            return additionalGame;
        }

        public AdditionalGame Delete(int id)
        {
            AdditionalGame additionalGame = context.AdditionalGames.Find(id);
            if (additionalGame != null)
            {
                context.AdditionalGames.Remove(additionalGame);
                context.SaveChanges();
            }
            return additionalGame;
        }

        public AdditionalGame GetAdditionalGame(int id)
        {
            return context.AdditionalGames.Find(id);
        }

        public IEnumerable<AdditionalGame> GetAllAdditionalGames()
        {
            return context.AdditionalGames
                            .Include(b => b.Barmaid)
                            .Include(i => i.Instructor)
                            .Include(gt => gt.GameType)
                            .OrderByDescending(ad => ad.Date);
        }

        public IQueryable<AdditionalGame> GetAllAdditionalGamesAsIQueryable(BranchOfficeEnum branchOffice)
        {
            return context.AdditionalGames
                            .Include(b => b.Barmaid)
                            .Include(i => i.Instructor)
                            .Include(gt => gt.GameType)
                            .Include(u => u.ApplicationUser)
                            .Where(ag => ag.BranchOffice == branchOffice)
                            .OrderByDescending(ad => ad.Date);
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return context.Employees.Where(e => e.IsValid == true);
        }

        public IEnumerable<GameType> GetAllGameTypes()
        {
            return context.GameTypes;
        }

        public IEnumerable<AdditionalGame> GetMonthAdditionalGames(DateTime month)
        {
            return context.AdditionalGames.Include(b => b.Barmaid)
                                          .Include(i => i.Instructor)
                                          .Include(gt => gt.GameType)
                                          .Where(ag => ag.Date.Month == month.Month);
        }

        public IQueryable<AdditionalGame> GetMonthAdditionalGamesAsIQueryable(DateTime month, BranchOfficeEnum branchOffice)
        {
            return context.AdditionalGames.Include(b => b.Barmaid)
                              .Include(i => i.Instructor)
                              .Include(gt => gt.GameType)
                              .Include(u => u.ApplicationUser)
                              .Where(ag => ag.Date.Month == month.Month && ag.BranchOffice == branchOffice)
                              .OrderByDescending(ag => ag.Date);
        }

        public AdditionalGame Update(AdditionalGame additionalGameChanges)
        {
            var additionalGame = context.AdditionalGames.Attach(additionalGameChanges);
            additionalGame.State = EntityState.Modified;
            context.SaveChanges();
            return additionalGameChanges;
        }
    }
}
