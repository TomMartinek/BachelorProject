using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    // implementace rozhraní IGameTypeRepository
    public class SQLGameTypeRepository : IGameTypeRepository
    {
        private readonly AppDbContext context;

        public SQLGameTypeRepository(AppDbContext context)
        {
            this.context = context;
        }

        public GameType Add(GameType gameType)
        {
            context.Add(gameType);
            context.SaveChanges();
            return gameType;
        }

        public GameType Delete(int id)
        {
            GameType gameType = context.GameTypes.Find(id);
            if (gameType != null)
            {
                context.GameTypes.Remove(gameType);
                context.SaveChanges();
            }
            return gameType;
        }

        public IEnumerable<GameType> GetAllGameTypes()
        {
            return context.GameTypes;
        }

        public GameType GetGameType(int id)
        {
            return context.GameTypes.Find(id);
        }

        public GameType Update(GameType gameTypeChanges)
        {
            var gameType = context.GameTypes.Attach(gameTypeChanges);
            gameType.State = EntityState.Modified;
            context.SaveChanges();
            return gameTypeChanges;
        }
    }
}
