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
    public class AdditionalGameController : Controller
    {
        private readonly IAdditionalGameRepository additionalGameRepostioty;
        private readonly IEmployeeRepository employeeRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public AdditionalGameController(IAdditionalGameRepository additionalGameRepostioty,
                                        IEmployeeRepository employeeRepository,
                                        UserManager<ApplicationUser> userManager)
        {
            this.additionalGameRepostioty = additionalGameRepostioty;
            this.employeeRepository = employeeRepository;
            this.userManager = userManager;
        }

        public async Task<ViewResult> Index(string selectedMonth, int? pageNumber, string deleteMessage)
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            IQueryable<AdditionalGame> additionalGames = null;

            if (!string.IsNullOrEmpty(selectedMonth))
            {
                additionalGames = additionalGameRepostioty
                    .GetMonthAdditionalGamesAsIQueryable(DateTime.Parse(selectedMonth), user.BranchOffice.Value);
            }
            else
            {
                additionalGames = additionalGameRepostioty.GetAllAdditionalGamesAsIQueryable(user.BranchOffice.Value);
            }

            ViewBag.DeleteMessage = deleteMessage;
            ViewBag.Month = selectedMonth;
            ViewBag.BranchOfficeName = user.BranchOffice.Value == BranchOfficeEnum.Branik ? "Braník" : "Holešovice";


            int pageSize = 5;
            return View(await PaginatedList<AdditionalGame>.CreateAsync(additionalGames.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpPost]
        public IActionResult DeleteAdditionalGame(int id, string selectedMonth)
        {
            string deleteMessage = null;

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

        // Old index method
        //public ViewResult Index(string selectedMonth)
        //{
        //    AdditionalGameIndexViewModel model = new AdditionalGameIndexViewModel();

        //    if (!string.IsNullOrEmpty(selectedMonth))
        //    {
        //        model.AdditionalGames = additionalGameRepostioty.GetMonthAdditionalGames(DateTime.Parse(selectedMonth));
        //        model.SelectedMonth = selectedMonth;
        //    }
        //    else
        //    {
        //        model.AdditionalGames = additionalGameRepostioty.GetAllAdditionalGames();
        //    }

        //    ViewBag.Month = selectedMonth;

        //    return View("~/Views/AdditionalGame/index.cshtml", model);
        //}

        [HttpGet]
        public ViewResult Create()
        {
            AdditionalGameViewModel model = new AdditionalGameViewModel();

            model.Date = DateTime.Now;
            model.Barmaids = GetBarmaidsToSelectList(true);
            model.Employees = GetEmployeesToSelectList(true);
            model.GameTypes = GetGameTypesToSelectList(true);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdditionalGameViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.ApplicationUserName);

            if (user == null)
            {
                ModelState.AddModelError("ApplicationUserName", "Chyba při zjískávání Id uživatele");
            }

            if (!model.BarmaidId.HasValue && !model.InstructorId.HasValue)
            {
                ModelState.AddModelError("BarmaidId", "Vyberte alespoň jednoho zaměstnance");
                ModelState.AddModelError("InstructorId", "Vyberte alespoň jednoho zaměstnance");
            }
            if ((model.BarmaidId == model.InstructorId) && (model.BarmaidId.HasValue || model.InstructorId.HasValue))
            {
                ModelState.AddModelError("BarmaidId", "Barmanka a intruktor nemohou být stejná osoba");
                ModelState.AddModelError("InstructorId", "Barmanka a intruktor nemohou být stejná osoba");
            }

            if (ModelState.IsValid)
            {
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

                additionalGameRepostioty.Add(newAdditionalGame);
                return RedirectToAction("index");
            }

            model.Barmaids = GetBarmaidsToSelectList(true);
            model.Employees = GetEmployeesToSelectList(true);
            model.GameTypes = GetGameTypesToSelectList(true);
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
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

            return View(additionalGameEditModel);
        }

        [HttpPost]
        public IActionResult Edit(AdditionalGameViewModel model)
        {
            if (!model.BarmaidId.HasValue && !model.InstructorId.HasValue)
            {
                ModelState.AddModelError("BarmaidId", "Vyberte alespoň jednoho zaměstnance");
                ModelState.AddModelError("InstructorId", "Vyberte alespoň jednoho zaměstnance");
            }

            if (ModelState.IsValid)
            {
                AdditionalGame additionalGame = additionalGameRepostioty.GetAdditionalGame(model.Id);
                additionalGame.Date = model.Date;
                additionalGame.Count = model.Count;
                additionalGame.GameTypeId = model.GameTypeId.Value;
                additionalGame.BarmaidId = model.BarmaidId;
                additionalGame.InstructorId = model.InstructorId;

                additionalGameRepostioty.Update(additionalGame);
                return RedirectToAction("index");
            }

            model.GameTypes = GetGameTypesToSelectList(false);
            model.Barmaids = GetBarmaidsToSelectList(false);
            model.Employees = GetEmployeesToSelectList(false);
            return View(model);
        }

        [HttpPost]
        public async Task<ViewResult> Summary(string selectedMonth)
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            DateTime month = DateTime.Parse(selectedMonth);
            IEnumerable<Employee> employees = employeeRepository.GetAllEmployees();
            IEnumerable<GameType> gameTypes = additionalGameRepostioty.GetAllGameTypes();

            AdditionalGamesOverviewViewModel model = new AdditionalGamesOverviewViewModel();

            model.SelectedDate = month;
            model.GameTypes = gameTypes;

            foreach (Employee employee in employees)
            {
                model.EmployeeGames.Add(GetEmployeeGames(employee, month, gameTypes, user.BranchOffice.Value));
            }

            ViewBag.SelectedMonth = selectedMonth;

            ViewBag.BranchOfficeName = user.BranchOffice.Value == BranchOfficeEnum.Branik ? "Braník" : "Holešovice";


            return View(model);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportToExcel(DateTime selectedDate)
        {
            await Task.Yield();

            var user = await userManager.FindByNameAsync(User.Identity.Name);
            string branchOfficeName = user.BranchOffice.Value == BranchOfficeEnum.Branik ? "Braník" : "Holešovice";


            IEnumerable<Employee> employees = employeeRepository.GetAllEmployees();
            IEnumerable<GameType> gameTypes = additionalGameRepostioty.GetAllGameTypes();

            AdditionalGamesOverviewViewModel model = new AdditionalGamesOverviewViewModel();

            model.SelectedDate = selectedDate;
            model.GameTypes = gameTypes;

            foreach (Employee employee in employees)
            {
                model.EmployeeGames.Add(GetEmployeeGames(employee, selectedDate, gameTypes, user.BranchOffice.Value));
            }

            var stream = new MemoryStream();
            string excelName = $"PremluveneHry-{branchOfficeName}-{DateTime.Now.ToString("yyyy-MM")}.xlsx";

            //using (var package = new ExcelPackage(stream))
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Přemluvené hry");

                int rowStart = 1;
                char columnStart = 'C';
                char columnEnd = 'C';

                for (int i = 0; i < gameTypes.Count()-1; i++)
                {
                    columnEnd++;
                }

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

                foreach (var item in gameTypes)
                {
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = string.Format("{0}", item.Name);
                    columnStart++;
                }
                foreach (var item in gameTypes)
                {
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = string.Format("{0}", item.Name);
                    columnStart++;
                }
                columnStart = 'A';
                columnEnd = 'A';
                rowStart++;

                foreach (var emp in model.EmployeeGames)
                {
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = string.Format("{0}", emp.Employee.Name);
                    columnStart++;
                    columnEnd++;
                    ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = emp.TotalCommision;
                    ws.Cell(string.Format("{0}{1}", columnStart, rowStart)).Style.NumberFormat.Format = "# ### kč";
                    columnStart++;
                    columnEnd++;
                    foreach (var duoGame in emp.SummedDuoGames)
                    {
                        ws.Cells(string.Format("{0}{1}", columnStart, rowStart)).Value = duoGame.Count;
                        columnStart++;
                        columnEnd++;
                    }
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


                ws.Columns(1,30).AdjustToContents();
                ws.Columns(1,30).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var colB = ws.Column("B");
                colB.Width = 10;

                workbook.SaveAs(stream);
            }
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        private EmployeeGamesModel GetEmployeeGames(Employee employee, DateTime month, IEnumerable<GameType> gameTypes, BranchOfficeEnum branchOffice) {
            EmployeeGamesModel employeeGames = new EmployeeGamesModel();
            List<SimplifiedGame> simplifiedSoloGames = new List<SimplifiedGame>();
            List<SimplifiedGame> simplifiedDuoGames = new List<SimplifiedGame>();

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

            int totalSoloCommision = 0;
            summedSoloGames.ForEach(g => {
                totalSoloCommision += g.Commision * g.Count;
            });
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