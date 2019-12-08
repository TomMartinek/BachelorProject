using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BachelorProject.Models;
using BachelorProject.ViewModels.User;
using BachelorProject.ViewModels.UserRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BachelorProject.Controllers
{
    /** 
        Tento konroler obsahuje logiku spojenou s administrací uživatelů a uživatelských rolí.
        Všechny metody této třídy jsou přístupny pouze užavateli s rolí "admin".
    */

    [Authorize(Roles = "admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        // konstruktor této třídy
        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        // metoda pro zobrazení pohledu obsahující všechny registrované uživatele
        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        // metoda pro zobrazení formaláře k úpravě uživatele
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            // zjískání uživatele z databáze
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"Uživatel s Id = {id} nebyl nalezen";
                return View("NotFound");
            }

            // zjískání rolí, kterým je tento uživatel přiřazen
            var userRoles = await userManager.GetRolesAsync(user);

            // naplnění modelu patřičnými daty
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                //UserName = user.UserName,
                BranchOffice = user.BranchOffice,
                Roles = userRoles,
            };

            return View(model);
        }

        // metoda pro odstranění uživatele
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // zjískání uživatele z DB podle jeho ID
            var user = await userManager.FindByIdAsync(id);

            // vhybová hláška pokud nebyl nalezen
            if (user == null)
            {
                ViewBag.ErrorMessage = $"Uživatel s Id = {id} nebyl nalezen";
                return View("NotFound");
            }
            else
            {
                try
                {
                    // pokus o odstranění uživatele
                    var result = await userManager.DeleteAsync(user);

                    if (result.Succeeded)
                    {
                        // pokud se operace zdařila je uživatel vrácen na přehled uživatelů
                        return RedirectToAction("ListUsers");
                    }
                    // pokud došlo k nějaké chybě jsou uživateli zobrazeny chyby a uživateli je zobrazen přehled uživatelů
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("ListUsers");
                }
                // pokud existují reference na tohoto uživatele v jiných entitách, je zobrazena hláška, že nemůže být ostraněn
                catch (DbUpdateException)
                {
                    ViewBag.ErrorTitle = $"Uživatel '{user.UserName}' je využíván";
                    ViewBag.ErrorMessage = $"Tento uživatel nemůže být smazán, protože mu jsou přiřazeny role, přemluvené hry nebo vouchery. " +
                                           $"Odeberte nejdříve uživateli role a odstraňte vouchery a přemluvené hry, které vytvořil a zkuste to znovu.";
                    return View("Error");
                }
            }
        }

        // metoda pro uložení upraveného uživatele do DB
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);

            // zobrazení chyby v případě, že nastala chyba při zjískávání uživatele z DB
            if (user == null)
            {
                ViewBag.ErrorMessage = $"Uživatel s Id = {model.Id} nebyl nalezen";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                //user.UserName = model.UserName;
                user.BranchOffice = model.BranchOffice;

                // uložení uživatele do DB
                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                // zobrazení chyb, pokud došlo k chybě při ukládání uživatele
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
            
        }

        // metoda pro zobrazení formuláře k přidávání/odebírání rolí uživateli
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            // získání uživatele z databáze
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"Uživatel s Id = {userId} nebyl nalezen";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            // získání všech uživatelských rolí z DB
            foreach (var role in roleManager.Roles)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                };
                // zjištění, zdali je uživatel této roli již přiřazen
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                // přidání této role do modelu
                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        // přidání/odebrání rolí uživateli po potvrzení formuláře
        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRoleViewModel> model, string userId)
        {
            // získání uživatele z DB
            var user = await userManager.FindByIdAsync(userId);

            // zobrazení chyby pokud se uživatele nepodařilo z DB získat
            if (user == null)
            {
                ViewBag.ErrorMessage = $"Uživatel s Id = {userId} nebyl nalezen";
                return View("NotFound");
            }

            // získání všech rolí z DB
            var roles = await userManager.GetRolesAsync(user);
            // odebrání uživateli všech rolí
            var result = await userManager.RemoveFromRolesAsync(user, roles);

            // zobrazení chybové hlášky došlo-li k chybě
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Stávající role nelze odstranit");
                return View(model);
            }

            // přidání uživateli všech označených rolí
            result = await userManager.AddToRolesAsync(user, 
                                model.Where(x => x.IsSelected).Select(y => y.Name));

            // zobrazení chybové hlášky došlo-li k chybě
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Vybrané role nelze přidat");
                return View(model);
            }

            // přesměrování na úpravu uživatele
            return RedirectToAction("EditUser", new { Id = userId});
        }

        // metoda pro zobrazení formuláře pro vytvoření role
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        // metoda pro odstranění role
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            // získání role z DB
            var role = await roleManager.FindByIdAsync(id);
            // zobrazení chybové hlášky, pokud se operace nezdařila
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role s Id = {id} nebyla nalezena";
                return View("NotFound");
            }
            else
            {
                // pokus o odstranění role z DB
                try
                {
                    // odstranění role z DB
                    var result = await roleManager.DeleteAsync(role);
                    // pokud se akce zdařile je uživatel vrácen na přehled rolí
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }
                    // došlo-li k chybě, jsou uživateli zobrazeny chyby
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("ListRoles");
                }
                // není-li možné tuto roli smazat z důvodů referencí na uživatele je zobrazena chybová hláška
                catch (DbUpdateException)
                {
                    ViewBag.ErrorTitle = $"Role '{role.Name}' je využívána";
                    ViewBag.ErrorMessage = $"Tato role nemůže být smazána, protože ji jsou přiřazeni uživatelé. " +
                                           $"Odeberte nejdříve z role uživatele a zkuste to znovu.";
                    return View("Error");
                }
            }
        }

        // metoda pro vytvoření uživatelské role
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            // kontrola modelu
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                // uložení role do DB
                IdentityResult result = await roleManager.CreateAsync(identityRole);

                // byla-li akce úspěšná, je uživateli zobrazen přehled rolí
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }

                // došlo-li k chybě jsou uživateli zobrazeny chyby
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
            
        }

        // metoda pro zobrazení všech uživatelských rolí
        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        // metoda pro zobrazení formuláře k úpravě uživatelské role
        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            // získání uživatelské role z DB
            var role = await roleManager.FindByIdAsync(id);
            // došlo-li k chybě, je uživateli zobrazena chybová hláška
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role s Id = {id} nebyla nalezena";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            // získání užavelů přiřazených této roli
            foreach(var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        // metoda pro uložení změněné uživatelské role
        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            // získání role z DB
            var role = await roleManager.FindByIdAsync(model.Id);
            // nebyla-li role nalezena, je zobrazena chybová hláška
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role s Id = {model.Id} nebyla nalezena";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                // uložení úpravy do DB
                var result = await roleManager.UpdateAsync(role);
                // byla-li akce úspěšná, je uživatel přesměrován na přehled uživatelských rolí
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                // zobrazení případných chyb
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        // metoda pro zobrazení formuláře k úpravě uživatelů dané role
        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            // získání role z DB
            var role = await roleManager.FindByIdAsync(roleId);
            // zobrazení případné chybové hlášky
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role s Id = {roleId} nebyla nalezena";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            // naplnění modelu daty o uživatelích a jejich přiřazení této roli
            foreach (var user in userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    Id = user.Id,
                    Name = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        // metoda ke změně přiřazenýchu živatelů vybrané roli po potvrzení formuláře
        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            // získání role z DB
            var role = await roleManager.FindByIdAsync(roleId);
            // zobrazení případné chybové hlášky
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role s Id = {roleId} nebyla nalezena";
                return View("NotFound");
            }

            // průchod jednotlivých uživatelů v modelu
            for (int i = 0; i < model.Count; i++)
            {
                // získání uživatele z DB
                var user = await userManager.FindByIdAsync(model[i].Id);

                IdentityResult result = null;

                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    // přidání uživatele roli, pokud byl tento uživatel označen
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    // odebrání uživatele rol, pokud nebyl tento uživatel označen
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }


    }
}