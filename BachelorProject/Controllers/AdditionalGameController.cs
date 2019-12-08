using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BachelorProject.Models;
using BachelorProject.ViewModels.AdditionalGameViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using ClosedXML.Excel;
using BachelorProject.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BachelorProject.Models.Enums;

namespace BachelorProject.Controllers
{
    /** 
        Tento konroler obsahuje logiku spojenou se systémem přemluvených her.
        Nepřihlášeným uživatelům jsou přístupny pouze metody s atributem "[AllowAnonymous]".
        Metody s atributem "[Authorize(Roles = "admin")]" jsou přístupné pouze administrátorovi.
        Zbylé metody jsou přístupné všem přihlášeným uživatelům.
    */

    public class AdditionalGameController : Controller
    {
        private readonly IAdditionalGameRepository additionalGameRepostioty;
        private readonly IEmployeeRepository employeeRepository;
        private readonly UserManager<ApplicationUser> userManager;
        
        // konstruktor této třídy
        public AdditionalGameController(IAdditionalGameRepository additionalGameRepostioty,
                                        IEmployeeRepository employeeRepository,
                                        UserManager<ApplicationUser> userManager)
        {
            this.additionalGameRepostioty = additionalGameRepostioty;
            this.employeeRepository = employeeRepository;
            this.userManager = userManager;
        }

        // metoda pro zobrazení přemluvených her dané pobočky, může se jednat o všechny, nebo ty z konkrétního měsíce
        public async Task<ViewResult> Index(string selectedMonth, int? pageNumber, string deleteMessage)
        {
            // zjištění, který uživatel je přihlášen pro filtrování přemluvených her podle pobočky
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            IQueryable<AdditionalGame> additionalGames = null;

            // zjištění, zadli mají být zobrazeny přemluvené hry pouze pro daný měsíc
            if (!string.IsNullOrEmpty(selectedMonth))
            {
                // zjískání přemluvených her dané pobočky pro daný měsíc z databáze
                additionalGames = additionalGameRepostioty
                    .GetMonthAdditionalGamesAsIQueryable(DateTime.Parse(selectedMonth), user.BranchOffice.Value);
            }
            else
            {
                // zjískání všech přemluvených her dané pobočky z databáze
                additionalGames = additionalGameRepostioty.GetAllAdditionalGamesAsIQueryable(user.BranchOffice.Value);
            }

            // předání dodatečných údajů pohledu
            ViewBag.DeleteMessage = deleteMessage;
            ViewBag.Month = selectedMonth;
            ViewBag.BranchOfficeName = user.BranchOffice.Value == BranchOfficeEnum.Branik ? "Braník" : "Holešovice";

            // počet záznamů na jedné straně
            int pageSize = 5;
            return View(await PaginatedList<AdditionalGame>.CreateAsync(additionalGames.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // metoda pro smazání přemluvené hry
        [HttpPost]
        public IActionResult DeleteAdditionalGame(int id, string selectedMonth)
        {
            string deleteMessage = null;

            // pokus o smazání záznamu a případné zobrazení chybové hlášky
            try
            {
                AdditionalGame additionalGame = additionalGameRepostioty.Delete(id);
                deleteMessage = "Přemluvená hra byla úspěšně smazána";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage, SelectedMonth = selectedMonth });
            }
            catch (DbUpdateException)
            {
                deleteMessage = "Přemluvenou hru se nepodařilo smazat";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage, SelectedMonth = selectedMonth });
            }
        }

        // metoda pro zobrazení formuláře k vytvoření přemluvené hry 
        [HttpGet]
        public ViewResult Create()
        {
            AdditionalGameViewModel model = new AdditionalGameViewModel();

            model.Date = DateTime.Now;
            // naplnění výběrových oken
            model.Barmaids = GetBarmaidsToSelectList(true);
            model.Employees = GetEmployeesToSelectList(true);
            model.GameTypes = GetGameTypesToSelectList(true);

            return View(model);
        }

        // metoda pro vytvoření přemluvené hry po potvrzení formuláře
        [HttpPost]
        public async Task<IActionResult> Create(AdditionalGameViewModel model)
        {
            // zjískání přihlášeného uživatele
            var user = await userManager.FindByNameAsync(model.ApplicationUserName);

            // kontrola zdali byl uživatel zjískán
            if (user == null)
            {
                ModelState.AddModelError("ApplicationUserName", "Chyba při zjískávání Id uživatele");
            }

            // kontrola zdali je vyplněn alespoň jeden zaměstnaec
            if (!model.BarmaidId.HasValue && !model.InstructorId.HasValue)
            {
                ModelState.AddModelError("BarmaidId", "Vyberte alespoň jednoho zaměstnance");
                ModelState.AddModelError("InstructorId", "Vyberte alespoň jednoho zaměstnance");
            }

            // kontrola předaného modelu
            if (ModelState.IsValid)
            {
                // vytvoření přemluvené hry podle předaného modelu
                AdditionalGame newAdditionalGame = new AdditionalGame
                {
                    Date = model.Date,
                    Count = model.Count,
                    GameTypeId = model.GameTypeId.Value,
                    BarmaidId = model.BarmaidId,
                    InstructorId = model.InstructorId,
                    ApplicationUserId = user.Id,
                    BranchOffice = user.BranchOffice.Value,
                };

                // uložení záznamu do databáze
                additionalGameRepostioty.Add(newAdditionalGame);
                // návrat zpět na metodu index
                return RedirectToAction("index");
            }

            // pokud nebyly hodnoty modelu platné je navrácen formulář pro vytvoření přemluvené hry a jsou opět načteny hodnoty pro výběrová pole
            model.Barmaids = GetBarmaidsToSelectList(true);
            model.Employees = GetEmployeesToSelectList(true);
            model.GameTypes = GetGameTypesToSelectList(true);
            return View(model);
        }

        // metoda pro zobrazení formuláře pro úpravu přemluvené hry
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // zjískání přemluvené hry z databáze
            AdditionalGame additionalGame = additionalGameRepostioty.GetAdditionalGame(id);
            AdditionalGameViewModel additionalGameEditModel = new AdditionalGameViewModel
            {
                Id = additionalGame.Id,
                Date = additionalGame.Date,
                Count = additionalGame.Count,
                GameTypeId = additionalGame.GameTypeId,
                BarmaidId = additionalGame.BarmaidId,
                InstructorId = additionalGame.InstructorId,
                GameTypes = GetGameTypesToSelectList(false),
                Barmaids = GetBarmaidsToSelectList(false),
                Employees = GetEmployeesToSelectList(false),
            };
            // zobrazení formuláře
            return View(additionalGameEditModel);
        }

        // metoda pro uložení upravené přemluvené hry po potvrzení formuláře
        [HttpPost]
        public IActionResult Edit(AdditionalGameViewModel model)
        {
            // kontrol, zdali je vybrán alespoň jeden zaměstnanec
            if (!model.BarmaidId.HasValue && !model.InstructorId.HasValue)
            {
                ModelState.AddModelError("BarmaidId", "Vyberte alespoň jednoho zaměstnance");
                ModelState.AddModelError("InstructorId", "Vyberte alespoň jednoho zaměstnance");
            }

            // kontrola předaného modelu
            if (ModelState.IsValid)
            {
                AdditionalGame additionalGame = additionalGameRepostioty.GetAdditionalGame(model.Id);
                additionalGame.Date = model.Date;
                additionalGame.Count = model.Count;
                additionalGame.GameTypeId = model.GameTypeId.Value;
                additionalGame.BarmaidId = model.BarmaidId;
                additionalGame.InstructorId = model.InstructorId;

                // uložení upravené přemluvené hry dodatabáze
                additionalGameRepostioty.Update(additionalGame);
                return RedirectToAction("index");
            }

            model.GameTypes = GetGameTypesToSelectList(false);
            model.Barmaids = GetBarmaidsToSelectList(false);
            model.Employees = GetEmployeesToSelectList(false);
            return View(model);
        }

        // metoda pro zobrazení výsledků přemluvených her za daný měsíc
        [HttpPost]
        public async Task<ViewResult> Summary(string selectedMonth)
        {
            // zjištění, který uživatel je přihlášen pro filtrování přemluvených her podle pobočky
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            DateTime month = DateTime.Parse(selectedMonth);
            // získání všech zaměstnanců z databáze
            IEnumerable<Employee> employees = employeeRepository.GetAllEmployees();
            // získání všech typů her z databáze
            IEnumerable<GameType> gameTypes = additionalGameRepostioty.GetAllGameTypes();

            // inicializace modelu
            AdditionalGamesOverviewViewModel model = new AdditionalGamesOverviewViewModel();

            model.SelectedDate = month;
            model.GameTypes = gameTypes;

            // uspořádání přemluvených her a výpočet provizí
            foreach (Employee employee in employees)
            {
                model.EmployeeGames.Add(GetEmployeeGames(employee, month, gameTypes, user.BranchOffice.Value));
            }

            ViewBag.SelectedMonth = selectedMonth;

            ViewBag.BranchOfficeName = user.BranchOffice.Value == BranchOfficeEnum.Branik ? "Braník" : "Holešovice";


            return View(model);
        }

        // metoda pro exportování výsledků přemluvených her za daný měsíc ve formátu XSLX
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportToExcel(DateTime selectedDate)
        {
            await Task.Yield();

            // získání přihlášeného uživatele pro filtrování přemluveenéch her podle pobočky
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            string branchOfficeName = user.BranchOffice.Value == BranchOfficeEnum.Branik ? "Braník" : "Holešovice";

            // získání všech zaměstnanců z databáze
            IEnumerable<Employee> employees = employeeRepository.GetAllEmployees();
            // získání všech typů her z databáze
            IEnumerable<GameType> gameTypes = additionalGameRepostioty.GetAllGameTypes();

            AdditionalGamesOverviewViewModel model = new AdditionalGamesOverviewViewModel();

            model.SelectedDate = selectedDate;
            model.GameTypes = gameTypes;

            // uspořádání přemluvených her a výpočet provizí
            foreach (Employee employee in employees)
            {
                model.EmployeeGames.Add(GetEmployeeGames(employee, selectedDate, gameTypes, user.BranchOffice.Value));
            }

            var stream = new MemoryStream();
            string excelName = $"PremluveneHry-{branchOfficeName}-{DateTime.Now.ToString("yyyy-MM")}.xlsx";

            // vytvoření XL workbooku
            using (var workbook = new XLWorkbook())
            {
                // přidání listu
                var ws = workbook.Worksheets.Add("Přemluvené hry");

                // počáteční hodnoty buněk pro zápis výsledků
                int rowStart = 1;
                char columnStart = 'C';
                char columnEnd = 'C';

                for (int i = 0; i < gameTypes.Count()-1; i++)
                {
                    columnEnd++;
                }

                // vyplnění a formátování názvů sloupců...
                ws.Cells("A1").Value = "Zaměstnanci";
                ws.Cells("B1").Value = "Celkové prémie";
                ws.Range("A1:A2").Merge();
                ws.Range("B1:B2").Merge();
                ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("B1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("B1").Style.Alignment.WrapText = true;

                ws.Cell(string.Format("{0}{1}", columnStart, rowStart)).Value = "Počty společných přemluvených her";
                ws.Range(string.Format("{0}{1}:{2}{3}", columnStart, rowStart, columnEnd, rowStart)).Merge();

                for (int i = 0; i < gameTypes.Count(); i++)
                {
                    columnStart++;
                    columnEnd++;
                }

                ws.Cell(string.Format("{0}{1}", columnStart, rowStart)).Value = "Počty sólo přemluvených her";
                ws.Range(string.Format("{0}{1}:{2}{3}", columnStart, rowStart, columnEnd, rowStart)).Merge();

                columnStart = 'C';
                rowStart++;

                // výpis typů her pro společné přemluvené hry
                foreach (var item in gameTypes)
                {
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = string.Format("{0}", item.Name);
                    columnStart++;
                }
                // výpis typů her pro sólo přemluvené hry
                foreach (var item in gameTypes)
                {
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = string.Format("{0}", item.Name);
                    columnStart++;
                }
                columnStart = 'A';
                columnEnd = 'A';
                rowStart++;

                // naplnění tabulky hodnotami provizí a počty přemluvených her sečtených podle typů her
                foreach (var emp in model.EmployeeGames)
                {
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = string.Format("{0}", emp.Employee.Name);
                    columnStart++;
                    columnEnd++;
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = emp.TotalCommision;
                    ws.Cell(string.Format("{0}{1}", columnStart, rowStart)).Style.NumberFormat.Format = "# ### kč";
                    columnStart++;
                    columnEnd++;
                    // počty společných přemluvených her
                    foreach (var duoGame in emp.SummedDuoGames)
                    {
                        ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = duoGame.Count;
                        columnStart++;
                        columnEnd++;
                    }
                    // počty sólo přemluvených her
                    foreach (var soloGame in emp.SummedSoloGames)
                    {
                        ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = soloGame.Count;
                        columnStart++;
                        columnEnd++;
                    }
                    columnStart = 'A';
                    rowStart++;
                }

                //var rngGamesCounts = ws.Range(string.Format("C3:{0}{1}", columnEnd, rowStart));
                //rngGamesCounts.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                //var rngCommision = ws.Range(string.Format("B3:B{0}", rowStart));
                //rngCommision.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // zarovnání hodnot v buňkách
                ws.Columns(1,30).AdjustToContents();
                ws.Columns(1,30).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var colB = ws.Column("B");
                colB.Width = 10;

                workbook.SaveAs(stream);
            }
            stream.Position = 0;
            // vrácení souboru užvateli
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        // metoda pro přeuspořádání dat a výpočet provizí z přemluvených her pro jednotlivé zaměstnance
        private EmployeeGamesModel GetEmployeeGames(Employee employee, DateTime month, IEnumerable<GameType> gameTypes, BranchOfficeEnum branchOffice) {
            EmployeeGamesModel employeeGames = new EmployeeGamesModel();
            List<SimplifiedGame> simplifiedSoloGames = new List<SimplifiedGame>();
            List<SimplifiedGame> simplifiedDuoGames = new List<SimplifiedGame>();

            // přemluvené hry v roli instruktor
            if (employee.Role == EmployeeRoleEnum.Instruktor)
            {
                foreach (AdditionalGame game in employee.InstuctorAdditionalGames.Where(ig => ig.Date.Month == month.Month && ig.BranchOffice == branchOffice))
                {
                    SimplifiedGame simplifiedGame = new SimplifiedGame(game);

                    if (game.BarmaidId.HasValue)
                        simplifiedDuoGames.Add(simplifiedGame);
                    else
                        simplifiedSoloGames.Add(simplifiedGame);
                }
            }
            // přemluvené hry v roli hlavní barmanka
            else if (employee.Role == EmployeeRoleEnum.Hlavni_barmanka)
            {
                foreach (AdditionalGame game in employee.BarmaidAdditionalGames.Where(bg => bg.Date.Month == month.Month && bg.BranchOffice == branchOffice))
                {
                    SimplifiedGame simplifiedGame = new SimplifiedGame(game);

                    if (game.InstructorId.HasValue)
                        simplifiedDuoGames.Add(simplifiedGame);
                    else
                        simplifiedSoloGames.Add(simplifiedGame);
                }
            }

            List<SummedGame> summedDuoGames = new List<SummedGame>();
            List<SummedGame> summedSoloGames = new List<SummedGame>();

            //Dictionary<int, int> soloGamesSummedByType = new Dictionary<int, int>();
            //Dictionary<int, int> duoGamesSummedByType = new Dictionary<int, int>();

            // součet počtů přemluvených her podle typů her
            foreach (GameType gameType in gameTypes)
            {
                int sumSolo = simplifiedSoloGames.Where(g => g.GameTypeId == gameType.Id).Sum(game => game.Count);
                summedSoloGames.Add(new SummedGame(gameType.Id, gameType.SoloCommision, sumSolo));

                if (employee.Role == EmployeeRoleEnum.Instruktor)
                {
                    int sumDuo = simplifiedDuoGames.Where(g => g.GameTypeId == gameType.Id).Sum(game => game.Count);
                    summedDuoGames.Add(new SummedGame(gameType.Id, gameType.InstruktorCommision, sumDuo));
                }
                if (employee.Role == EmployeeRoleEnum.Hlavni_barmanka)
                {
                    int sumDuo = simplifiedDuoGames.Where(g => g.GameTypeId == gameType.Id).Sum(game => game.Count);
                    summedDuoGames.Add(new SummedGame(gameType.Id, gameType.BarmaidCommision, sumDuo));
                }
            }

            // výpočet provize ze "sólo" přemluvených her
            int totalSoloCommision = 0;
            summedSoloGames.ForEach(g => {
                totalSoloCommision += g.Commision * g.Count;
            });
            // výpořet provize ze "společných" přemluvených her
            int totalDuoCommision = 0;
            summedDuoGames.ForEach(g => {
                totalDuoCommision += g.Commision * g.Count;
            });

            employeeGames.Employee = employee;
            employeeGames.SummedSoloGames = summedSoloGames;
            employeeGames.SummedDuoGames = summedDuoGames;
            employeeGames.TotalCommision = totalSoloCommision + totalDuoCommision;

            return employeeGames;
        }

        // metody pro naplnění výběrových oken při vytváření/úpravě přemluvených her

        private List<SelectListItem> GetBarmaidsToSelectList(bool onlyValid)
        {
            if (onlyValid)
            {
                return additionalGameRepostioty.GetAllEmployees().Where(e => e.Role == EmployeeRoleEnum.Hlavni_barmanka && e.IsValid == true)
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }
            else
            {
                return additionalGameRepostioty.GetAllEmployees().Where(e => e.Role == EmployeeRoleEnum.Hlavni_barmanka)
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }

        }

        private List<SelectListItem> GetEmployeesToSelectList(bool onlyValid)
        {
            if (onlyValid)
            {
                return additionalGameRepostioty.GetAllEmployees().Where(e => e.Role != EmployeeRoleEnum.Hlavni_barmanka && e.IsValid == true)
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }
            else
            {
                return additionalGameRepostioty.GetAllEmployees().Where(e => e.Role != EmployeeRoleEnum.Hlavni_barmanka)
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }

        }

        private List<SelectListItem> GetGameTypesToSelectList(bool onlyValid)
        {
            if (onlyValid)
            {
                return additionalGameRepostioty.GetAllGameTypes().Where(gt => gt.IsValid == true)
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }
            else
            {
                return additionalGameRepostioty.GetAllGameTypes()
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }

        }

    }
}