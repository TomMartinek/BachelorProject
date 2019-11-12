using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BachelorProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BachelorProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class GameTypeController : Controller
    {
        private readonly IGameTypeRepository gameTypeRepository;

        public GameTypeController(IGameTypeRepository gameTypeRepository)
        {
            this.gameTypeRepository = gameTypeRepository;
        }

        [HttpGet]
        public ViewResult Index(string deleteMessage)
        {
            ViewBag.DeleteMessage = deleteMessage;
            var model = gameTypeRepository.GetAllGameTypes().OrderByDescending(gt => gt.IsValid);

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteGameType(int id)
        {
            string deleteMessage = null;

            try
            {
                GameType gameType = gameTypeRepository.Delete(id);
                deleteMessage = "Typ hry byl úspěšně smazán";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage });
            }
            catch (DbUpdateException)
            {
                try
                {
                    GameType gameType = gameTypeRepository.GetGameType(id);
                    gameType.IsValid = false;
                    gameTypeRepository.Update(gameType);

                    deleteMessage = "Typ hry memůže být smazán protože je využíván. " +
                                    "Typ hry nelze od teď využívat při vytváření nových přemluvených her. " +
                                    "Pokud chcete tento typ hry smazat, odstraňte přemluvené hry s tímto typem a zkuste to znovu.";

                    return RedirectToAction("index", new { DeleteMessage = deleteMessage });
                }
                catch (DbUpdateException)
                {

                    deleteMessage = "Typ hry se nepodařilo smazat";
                    return RedirectToAction("index", new { DeleteMessage = deleteMessage });
                }
            }
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(GameType model)
        {
            if (ModelState.IsValid)
            {
                GameType newGameType = new GameType
                {
                    Name = model.Name,
                    Price = model.Price,
                    BarmaidCommision = model.BarmaidCommision,
                    InstruktorCommision = model.InstruktorCommision,
                    SoloCommision = model.SoloCommision,
                    IsValid = true,
                };

                gameTypeRepository.Add(newGameType);
                return RedirectToAction("index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            GameType gameType = gameTypeRepository.GetGameType(id);
            GameType gameTypeEditModel = new GameType
            {
                Id = gameType.Id,
                Name = gameType.Name,
                Price = gameType.Price,
                BarmaidCommision = gameType.BarmaidCommision,
                InstruktorCommision = gameType.InstruktorCommision,
                SoloCommision = gameType.SoloCommision,
                IsValid = gameType.IsValid
            };
            return View(gameTypeEditModel);
        }

        [HttpPost]
        public IActionResult Edit(GameType model)
        {
            if (ModelState.IsValid)
            {
                GameType gameType = gameTypeRepository.GetGameType(model.Id);
                gameType.Name = model.Name;
                gameType.Price = model.Price;
                gameType.BarmaidCommision = model.BarmaidCommision;
                gameType.InstruktorCommision = model.InstruktorCommision;
                gameType.SoloCommision = model.SoloCommision;
                gameType.IsValid = model.IsValid;

                gameTypeRepository.Update(gameType);
                return RedirectToAction("index");
            }

            return View();
        }
    }
}