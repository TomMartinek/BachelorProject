using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BachelorProject.Models;
using BachelorProject.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BachelorProject.Controllers
{
    /** 
        Tento konroler obsahuje logiku spojenou s přihlašováním a registrováním uživatelů.
        Nepřihlášeným uživatelům jsou přístupny pouze metody s atributem "[AllowAnonymous]".
        Metody s atributem "[Authorize(Roles = "admin")]" jsou přístupné pouze administrátorovi.
        Zbylé metody jsou přístupné přihlášeným uživatelům.
    */

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        // konstruktor této třídy
        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }


        // metoda pro odhlášení uživatele z aplikace
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "employee");
        }

        // metoda pro zobrazení registračního formuláře, kterou může provést pouze admin
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Register()
        {
            return View();
        }

        // metoda pro vytvoření uživatele po potvrzení registračního formuláře
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // kontrola platnosti předaného modelu
            if (ModelState.IsValid)
            {
                //vytvoření proměnné user s příslušnými daty
                var user = new ApplicationUser {
                    UserName = model.Email,
                    Email = model.Email,
                    BranchOffice = model.BranchOffice
                };
                //uložení uživatele
                var result = await userManager.CreateAsync(user, model.Password);

                //kontrola výsledku akce a případné přesměrování uživatele na seznam uživatelů, nebo zobrazení chybové hlášky
                if (result.Succeeded)
                {
                    if(signInManager.IsSignedIn(User) && User.IsInRole("admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "employee");
                }

                //zobrazení chybových hlášek
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // kontrola zadávaného emailu při registraci nového uživatele - není možné vytvořit uživatele se stejným emailem
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            //vyhledání uživatele podle emailu v databázi
            var user = await userManager.FindByEmailAsync(email);

            // pokud byl nalezen uživatel s tímto emailem, je vrácena chybová hláška
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Pro email {email} účet již existuje");
            }
        }

        // metotoda pro zobrazení přihlašovacího formuláře do aplikace
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // metoda pro přihlášení uživatele po potvrzení přihlašovacího formuláře
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // kontrola platnosti předaného modelu
            if (ModelState.IsValid)
            {
                //přihlášení uživatele
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                // ověření výsledku a případné přihlášení, nebo zobrazení chybové hlášky
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "additionalGame");
                }

                ModelState.AddModelError(string.Empty, "Přihlášení se nezdařilo");
            }

            return View(model);
        }

        // zobrazení chybové hlášky v případě, že by chtěl uživatel provést akci ke které nemá oprávnění
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}