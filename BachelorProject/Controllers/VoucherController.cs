using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BachelorProject.Models;
using BachelorProject.Utility;
using BachelorProject.ViewModels.VoucherViewModels;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BachelorProject.Controllers
{
    public class VoucherController : Controller
    {
        private readonly IVoucherRepository voucherRepository;
        private readonly IConverter converter;
        private readonly UserManager<ApplicationUser> userManager;

        public VoucherController(IVoucherRepository voucherRepository, 
                                 IConverter converter,
                                 UserManager<ApplicationUser> userManager)
        {
            this.voucherRepository = voucherRepository;
            this.converter = converter;
            this.userManager = userManager;
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> Index(string voucherCode, int? pageNumber, string deleteMessage)
        {
            if (!string.IsNullOrEmpty(voucherCode))
            {
                Voucher voucher = voucherRepository.FindVoucherByCode(voucherCode);
                if (voucher != null)
                {
                    return RedirectToAction("Details", "Voucher", new { id = voucher.Id });
                }
                else
                {
                    ViewBag.ErrorMessage = $"Voucher s kódem '{voucherCode}' nebyl nalezen.";
                }
            }

            ViewBag.DeleteMessage = deleteMessage;

            IQueryable<Voucher> vouchers = voucherRepository.GetAllVouchersAsIQueriable();

            int pageSize = 6;
            return View(await PaginatedList<Voucher>.CreateAsync(vouchers.AsNoTracking(), pageNumber ?? 1, pageSize));

        }

        [HttpPost]
        public IActionResult ApplyVoucher(int id)
        {
            Voucher voucher = voucherRepository.GetVoucher(id);

            if (!voucher.IsValid)
            {
                ViewBag.ErrorMessage = $"Tento voucher již byl dříve uplatněn a nelze ho tedy znovu uplatnit.";
                return View("~/Views/Voucher/Details.cshtml", voucher);
            }
            else
            {
                if ((voucher.ValidFrom.Date <= DateTime.Now.Date) && (DateTime.Now.Date <= voucher.ValidUntil.Date))
                {
                    voucher.IsValid = false;
                    voucherRepository.Update(voucher);
                    ViewBag.VoucherAppliedMessage = $"Voucher byl úspěšně uplatněn.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }
                else if (voucher.ValidFrom.Date > DateTime.Now.Date)
                {
                    ViewBag.ErrorMessage = $"Voucher je platný až od {voucher.ValidFrom.ToShortDateString()} a do té doby jej nelze uplatnit.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }
                else if (DateTime.Now.Date > voucher.ValidUntil.Date)
                {
                    ViewBag.ErrorMessage = $"Voucheru vypršela {voucher.ValidUntil.ToShortDateString()} platnost a nelze jej tedy uplatnit.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }
                else
                {
                    ViewBag.ErrorMessage = "Voucher nelze uplatnit.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }

            }
        }

        [HttpGet]
        public ViewResult Details(int id)
        {
            Voucher voucher = voucherRepository.GetVoucher(id);

            return View(voucher);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteVoucher(int id)
        {
            string deleteMessage = null;

            try
            {
                Voucher voucher = voucherRepository.Delete(id);
                deleteMessage = "Voucher byl úspěšně smazán";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage});
            }
            catch (DbUpdateException)
            {
                deleteMessage = $"Voucher se nepodařilo smazat";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage });
            }
        }

        //old Index method
        //public IActionResult Index(string voucherCode)
        //{
        //    if (!string.IsNullOrEmpty(voucherCode))
        //    {
        //        Voucher voucher = voucherRepository.FindVoucherByCode(voucherCode);
        //        if (voucher != null)
        //        {
        //            return RedirectToAction("Details", "Voucher", new { id = voucher.Id });
        //        }
        //        else
        //        {
        //            ViewBag.ErrorMessage = $"Voucher s kódem '{voucherCode}' nebyl nalezen.";
        //        }
        //    }

        //    var model = voucherRepository.GetAllVouchers();
        //    return View(model);

        //}

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ViewResult Create()
        {
            VoucherViewModel model = new VoucherViewModel();

            model.ValidFrom = DateTime.Now;
            model.ValidUntil = DateTime.Now;
            model.VoucherTypes = GetVoucherTypesToList(true);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.ApplicationUserName);

            if (user == null)
            {
                ModelState.AddModelError("ApplicationUserName", "Chyba při zjískávání Id uživatele");
            }

            if (model.ValidFrom > model.ValidUntil)
            {
                ModelState.AddModelError("ValidFrom", "Datum začátku platnosti musí být dříve než datum konce platnosti");
                ModelState.AddModelError("ValidUntil", "Datum konce platnosti musí být později než datum začátku platnosti");
            }

            if (ModelState.IsValid)
            {
                Voucher newVoucher = new Voucher
                {
                    ValidFrom = model.ValidFrom,
                    ValidUntil = model.ValidUntil,
                    Title = model.Title,
                    Description = model.Description,
                    Value = model.Value,
                    VoucherTypeId = model.VoucherTypeId.Value,

                    Code = (DateTime.Now.Ticks - new DateTime(2016, 1, 1).Ticks).ToString("x"),
                    CreationDate = DateTime.Now,
                    IsValid = true,
                    ApplicationUserId = user.Id
                };

                voucherRepository.Add(newVoucher);
                return RedirectToAction("details", new { id = newVoucher.Id });
            }

            model.VoucherTypes = GetVoucherTypesToList(true);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Edit(int id)
        {
            Voucher voucher = voucherRepository.GetVoucher(id);

            VoucherViewModel voucherViewModel = new VoucherViewModel
            {
                Id = voucher.Id,
                ValidFrom = voucher.ValidFrom,
                ValidUntil = voucher.ValidUntil,
                Title = voucher.Title,
                Description = voucher.Description,
                Value = voucher.Value,
                VoucherTypeId = voucher.VoucherTypeId,
                VoucherTypes = GetVoucherTypesToList(false),
            };
            return View(voucherViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Edit(VoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                Voucher voucher = voucherRepository.GetVoucher(model.Id);

                voucher.ValidFrom = model.ValidFrom;
                voucher.ValidUntil = model.ValidUntil;
                voucher.Title = model.Title;
                voucher.Description = model.Description;
                voucher.Value = model.Value;
                voucher.VoucherTypeId = model.VoucherTypeId.Value;

                voucherRepository.Update(voucher);
                return RedirectToAction("index");
            }

            model.VoucherTypes = GetVoucherTypesToList(false);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult ExportToPdf(int id)
        {
            Voucher voucher = voucherRepository.GetVoucher(id);

            var globalSettings = new GlobalSettings
            {
                Margins = new MarginSettings { Top = 0, Bottom = 0, Right = 0, Left = 0 },
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.MonarchEnvelope,
                DocumentTitle = "Voucher",
                Outline = false,
            };

            var objectSettings = new ObjectSettings
            {
                HtmlContent = VoucherGenerator.GetHTMLString(voucher),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css", "pdfStyles.css") },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };

            var file = converter.Convert(pdf);
            return File(file, "application/pdf", $"voucher-{voucher.Code}.pdf");
        }

        private List<SelectListItem> GetVoucherTypesToList(bool onlyValid)
        {
            if (onlyValid)
            {
                return voucherRepository.GetAllVoucherTypes().Where(vt => vt.IsValid == true)
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }
            else
            {
                return voucherRepository.GetAllVoucherTypes()
                        .Select(a => new SelectListItem()
                        {
                            Value = a.Id.ToString(),
                            Text = a.Name,
                        }).ToList();
            }

        }

    }
}