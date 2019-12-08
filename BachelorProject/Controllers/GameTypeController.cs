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
    /** 
        Tento konroler obsahuje logiku spojenou s administrací typů her.
        Všechny metody této třídy jsou přístupny pouze užavateli s rolí "admin".
    */

    [Authorize(Roles = "admin")]
    public class GameTypeController : Controller
    {
        private readonly IGameTypeRepository gameTypeRepository;

        public GameTypeController(IGameTypeRepository gameTypeRepository)
        {
            this.gameTypeRepository = gameTypeRepository;
        }

        // metoda pro zobrazení všech typů her uložených v DB
        [HttpGet]
        public ViewResult Index(string deleteMessage)
        {
            ViewBag.DeleteMessage = deleteMessage;
            // získání a seřazení všech typů her z DB podle jejich platnosti
            var model = gameTypeRepository.GetAllGameTypes().OrderByDescending(gt => gt.IsValid);

            return View(model);
        }

        // metoda pro odstranění typu hry z DB
        [HttpPost]
        public IActionResult DeleteGameType(int id)
        {
            string deleteMessage = null;

            // pokus o odstranění typu hry z DB
            try
            {
                // odstranění typu hry
                GameType gameType = gameTypeRepository.Delete(id);
                deleteMessage = "Typ hry byl úspěšně smazán";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage });
            }
            // odchycení chyby v případě, že tento typ hry je využit v nějaké přemluvené hře
            catch (DbUpdateException)
            {
                // pokus o změnu platnosti typu hry
                try
                {
                    GameType gameType = gameTypeRepository.GetGameType(id);
                    gameType.IsValid = false;
                    // uložení změny
                    gameTypeRepository.Update(gameType);

                    // zobrazení hlášky
                    deleteMessage = "Typ hry memůže být smazán protože je využíván. " +
                                    "Typ hry nelze od teď využívat při vytváření nových přemluvených her. " +
                                    "Pokud chcete tento typ hry smazat, odstraňte přemluvené hry s tímto typem a zkuste to znovu.";
                    // přesměrování uživatele
                    return RedirectToAction("index", new { DeleteMessage = deleteMessage });
                }
                catch (Exception)
                {
                    // odchycení neočekávané chyby při úpravě typu hry na neplatný stav
                    deleteMessage = "Typ hry se nepodařilo smazat";
                    return RedirectToAction("index", new { DeleteMessage = deleteMessage });
                }
            }
        }

        // metoda pro zobrazení formuláře pro vytvoření typu hry
        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        // metoda pro uložení nového typu hry do DB
        [HttpPost]
        public IActionResult Create(GameType model)
        {
            // kontrola předávaného modelu
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
                // uložení typu hry do DB
                gameTypeRepository.Add(newGameType);
                return RedirectToAction("index");
            }

            return View();
        }

        // metoda pro zobrazení formuláře pro úpravu typu hry
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // získání typu hry z DB
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

        // metoda pro uložení upraveného typu hry
        [HttpPost]
        public IActionResult Edit(GameType model)
        {
            // kontrola platnosti předaného modelu
            if (ModelState.IsValid)
            {
                GameType gameType = gameTypeRepository.GetGameType(model.Id);
                gameType.Name = model.Name;
                gameType.Price = model.Price;
                gameType.BarmaidCommision = model.BarmaidCommision;
                gameType.InstruktorCommision = model.InstruktorCommision;
                gameType.SoloCommision = model.SoloCommision;
                gameType.IsValid = model.IsValid;

                // uložení typu hry do Db
                gameTypeRepository.Update(gameType);
                // přesměrování uživatele na přehled zaměstnanců
                return RedirectToAction("index");
            }

            return View();
        }
    }
}