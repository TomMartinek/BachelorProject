using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public interface IGameTypeRepository
    {
        GameType GetGameType(int id);
        IEnumerable<GameType> GetAllGameTypes();
        GameType Add(GameType gameType);
        GameType Update(GameType gameTypeChanges);
        GameType Delete(int id);
    }
}
