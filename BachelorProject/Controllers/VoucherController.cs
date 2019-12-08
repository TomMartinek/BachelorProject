using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BachelorProject.Models;
using BachelorProject.Utility;
using BachelorProject.ViewModels.VoucherViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BachelorProject.Controllers
{
    /** 
        Tento konroler obsahuje logiku spojenou se systémem voucherů.
        Nepřihlášeným uživatelům jsou přístupny pouze metody s atributem "[AllowAnonymous]".
        Metody s atributem "[Authorize(Roles = "admin")]" jsou přístupné pouze administrátorovi.
        Zbylé metody jsou přístupné všem přihlášeným uživatelům.
    */

    public class VoucherController : Controller
    {
        private readonly IVoucherRepository voucherRepository;
        //private readonly IConverter converter;
        private readonly UserManager<ApplicationUser> userManager;

        // konstruktor této třídy
        public VoucherController(IVoucherRepository voucherRepository,
                                 //IConverter converter,
                                 UserManager<ApplicationUser> userManager)
        {
            this.voucherRepository = voucherRepository;
            //this.converter = converter;
            this.userManager = userManager;
        }

        // metoda pro zobrazní, nebo vyhledávání vocherů uložených v DB
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> Index(string voucherCode, int? pageNumber, string deleteMessage)
        {
            // kontrola zda-li si uživatel přeje vyhledat voucher 
            if (!string.IsNullOrEmpty(voucherCode))
            {
                // vyhledání voucheru v DB podle kódu
                Voucher voucher = voucherRepository.FindVoucherByCode(voucherCode);
                if (voucher != null)
                {
                    // přesměrování uživatele na detail voucheru
                    return RedirectToAction("Details", "Voucher", new { id = voucher.Id });
                }
                else
                {
                    // zobrazeníchybové hlášky, nebyl-li voucher nalezen
                    ViewBag.ErrorMessage = $"Voucher s kódem '{voucherCode}' nebyl nalezen.";
                }
            }

            ViewBag.DeleteMessage = deleteMessage;

            // nechtěl-li uživatel voucher vyhledat, jsou mu zobrazeny všechnyvouchery
            // získání všech voucherů z DB
            IQueryable<Voucher> vouchers = voucherRepository.GetAllVouchersAsIQueriable();

            // počet voucherů na jedné straně
            int pageSize = 6;
            return View(await PaginatedList<Voucher>.CreateAsync(vouchers.AsNoTracking(), pageNumber ?? 1, pageSize));

        }

        // metoda pro uplatnění voucheru
        [HttpPost]
        public IActionResult ApplyVoucher(int id)
        {
            // získání voucheru z DB
            Voucher voucher = voucherRepository.GetVoucher(id);

            // kontrola zda-li nebyl voucher již uplatněn
            if (!voucher.IsValid)
            {
                ViewBag.ErrorMessage = $"Tento voucher již byl dříve uplatněn a nelze ho tedy znovu uplatnit.";
                return View("~/Views/Voucher/Details.cshtml", voucher);
            }
            else
            {
                // kontrola dat platnosti voucheru
                if ((voucher.ValidFrom.Date <= DateTime.Now.Date) && (DateTime.Now.Date <= voucher.ValidUntil.Date))
                {
                    // uplatnění voucheru
                    voucher.IsValid = false;
                    voucherRepository.Update(voucher);
                    ViewBag.VoucherAppliedMessage = $"Voucher byl úspěšně uplatněn.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }
                // zobrazení chybové hlášky, v případě že voucher ještě není platný
                else if (voucher.ValidFrom.Date > DateTime.Now.Date)
                {
                    ViewBag.ErrorMessage = $"Voucher je platný až od {voucher.ValidFrom.ToShortDateString()} a do té doby jej nelze uplatnit.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }
                // zobrazení chybové hlášky, v případě že je voucher již prošlý 
                else if (DateTime.Now.Date > voucher.ValidUntil.Date)
                {
                    ViewBag.ErrorMessage = $"Voucheru vypršela {voucher.ValidUntil.ToShortDateString()} platnost a nelze jej tedy uplatnit.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }
                // zobrazení obecné chyby
                else
                {
                    ViewBag.ErrorMessage = "Voucher nelze uplatnit.";
                    return View("~/Views/Voucher/Details.cshtml", voucher);
                }

            }
        }

        // metoda pro zobrazení detailu voucheru
        [HttpGet]
        public ViewResult Details(int id)
        {
            // získání voucheru z DB
            Voucher voucher = voucherRepository.GetVoucher(id);

            return View(voucher);
        }

        // metoda pro odstranění voucheru z DB
        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteVoucher(int id)
        {
            string deleteMessage = null;

            // pokus o odstranění voucheru z DB
            try
            {
                // odstranění voucheru z DB
                Voucher voucher = voucherRepository.Delete(id);
                deleteMessage = "Voucher byl úspěšně smazán";
                // přesměrování uživatele na přehled voucherů
                return RedirectToAction("index", new { DeleteMessage = deleteMessage });
            }
            // odchycení případné chyby spojené s chybou v DB při odstraňování voucherz
            catch (DbUpdateException)
            {
                deleteMessage = $"Voucher se nepodařilo smazat";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage });
            }
        }

        // metoda pro zobrazení formuláře k vytvoření voucheru
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ViewResult Create()
        {
            VoucherViewModel model = new VoucherViewModel();

            // předvyplnění dat platnosti podle současného data
            model.ValidFrom = DateTime.Now;
            model.ValidUntil = DateTime.Now;
            //model.VoucherTypes = GetVoucherTypesToList(true);

            return View(model);
        }

        // metoda pro vytvoření voucheru
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
            // získání přihlášeného uživatele
            var user = await userManager.FindByNameAsync(model.ApplicationUserName);
            // zobrazení chyby v případě, že se uživatele nepodařilo získat
            if (user == null)
            {
                ModelState.AddModelError("ApplicationUserName", "Chyba při zjískávání Id uživatele");
            }
            // kontrola datumů začátku a konce platnosti 
            if (model.ValidFrom > model.ValidUntil)
            {
                ModelState.AddModelError("ValidFrom", "Datum začátku platnosti musí být dříve než datum konce platnosti");
                ModelState.AddModelError("ValidUntil", "Datum konce platnosti musí být později než datum začátku platnosti");
            }

            // kontrola platnosti předaného modelu
            if (ModelState.IsValid)
            {
                Voucher newVoucher = new Voucher
                {
                    ValidFrom = model.ValidFrom,
                    ValidUntil = model.ValidUntil,
                    Title = model.Title,
                    Description = model.Description,
                    Value = model.Value,
                    //VoucherTypeId = model.VoucherTypeId.Value,
                    // vygenerování pseoudonáhodného kódu voucheru
                    Code = (DateTime.Now.Ticks - new DateTime(2016, 1, 1).Ticks).ToString("x"),
                    CreationDate = DateTime.Now,
                    IsValid = true,
                    ApplicationUserId = user.Id
                };
                // uložení voucheru do DB
                voucherRepository.Add(newVoucher);
                return RedirectToAction("details", new { id = newVoucher.Id });
            }

            //model.VoucherTypes = GetVoucherTypesToList(true);
            return View(model);
        }

        // metoda pro zobrazení formuláře k upravení voucheru
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Edit(int id)
        {
            // získání vouceru z DB
            Voucher voucher = voucherRepository.GetVoucher(id);

            VoucherViewModel voucherViewModel = new VoucherViewModel
            {
                Id = voucher.Id,
                ValidFrom = voucher.ValidFrom,
                ValidUntil = voucher.ValidUntil,
                Title = voucher.Title,
                Description = voucher.Description,
                Value = voucher.Value,
                //VoucherTypeId = voucher.VoucherTypeId,
                //VoucherTypes = GetVoucherTypesToList(false),
            };
            return View(voucherViewModel);
        }

        // metoda pro uložení upraveného voucheru
        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Edit(VoucherViewModel model)
        {
            // kontrola platnosti předaného modelu
            if (ModelState.IsValid)
            {
                Voucher voucher = voucherRepository.GetVoucher(model.Id);

                voucher.ValidFrom = model.ValidFrom;
                voucher.ValidUntil = model.ValidUntil;
                voucher.Title = model.Title;
                voucher.Description = model.Description;
                voucher.Value = model.Value;
                //voucher.VoucherTypeId = model.VoucherTypeId.Value;

                // uložení upraveného voucheru do DB
                voucherRepository.Update(voucher);
                return RedirectToAction("index");
            }

            //model.VoucherTypes = GetVoucherTypesToList(false);
            return View(model);
        }

        // metoda pro exportování voucheru do souboru PDF
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult ExportToPdf(int id)
        {
            // získání voucheru z DB
            Voucher voucher = voucherRepository.GetVoucher(id);

            // vytvoření PDF dokumentu a načtení jeho pozadí
            PdfDocument document = PdfReader.Open(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "voucherBackground.pdf"), PdfDocumentOpenMode.Modify);
            PdfPage page = document.Pages[0];

            // stylování textu jednotlivých údajů
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont fontTitle = new XFont("Times New Roman", 25, XFontStyle.Bold);
            XFont fontDescription = new XFont("Times New Roman", 15, XFontStyle.Bold);
            XFont fontElse = new XFont("Times New Roman", 12, XFontStyle.Bold);
            XTextFormatter tf = new XTextFormatter(gfx);

            // příprava pozicí jednotlivých oken pro zápis údajů z voucheru

            //title position
            XRect rectTitle = new XRect(350, 350, 100, 40);
            //description position
            XRect rectDescription = new XRect(100, 420, 500, 40);
            //Valid from text position
            XRect rectValidFromText = new XRect(100, 470, 100, 40);
            //Valid until text position
            XRect rectValidUntilText = new XRect(100, 500, 100, 40);
            //Code text position
            XRect rectCodeText = new XRect(300, 500, 100, 40);
            //Value text position
            XRect rectValueText = new XRect(300, 470, 100, 40);
            //Valid from position
            XRect rectValidFrom = new XRect(200, 470, 100, 40);
            //Valid until position
            XRect rectValidUntil = new XRect(200, 500, 100, 40);
            //Value position
            XRect rectValue = new XRect(400, 470, 100, 40);
            //Code position
            XRect rectCode = new XRect(400, 500, 200, 40);

            // vykreslení a naplnění předpřipravených polí

            tf.DrawString(voucher.Title, fontTitle, XBrushes.LightCyan, rectTitle);
            tf.DrawString(voucher.Description, fontDescription, XBrushes.LightCyan, rectDescription);

            tf.DrawString("Platný od:", fontElse, XBrushes.LightCyan, rectValidFromText);
            tf.DrawString("Platný do:", fontElse, XBrushes.LightCyan, rectValidUntilText);
            tf.DrawString("Celková hodnota:", fontElse, XBrushes.LightCyan, rectValueText);
            tf.DrawString("Kód:", fontElse, XBrushes.LightCyan, rectCodeText);

            tf.DrawString(voucher.ValidFrom.ToShortDateString(), fontElse, XBrushes.LightCyan, rectValidFrom);
            tf.DrawString(voucher.ValidUntil.ToShortDateString(), fontElse, XBrushes.LightCyan, rectValidUntil);
            tf.DrawString(string.Format("{0} Kč", voucher.Value), fontElse, XBrushes.LightCyan, rectValue);
            tf.DrawString(voucher.Code, fontElse, XBrushes.LightCyan, rectCode);




            byte[] fileContents = null;
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream, true);
                fileContents = stream.ToArray();
            }

            // vrácení souboru uživateli
            return File(fileContents, "application/pdf", $"Voucher-{voucher.Code}.pdf");
        }

        //private List<SelectListItem> GetVoucherTypesToList(bool onlyValid)
        //{
        //    if (onlyValid)
        //    {
        //        return voucherRepository.GetAllVoucherTypes().Where(vt => vt.IsValid == true)
        //                .Select(a => new SelectListItem()
        //                {
        //                    Value = a.Id.ToString(),
        //                    Text = a.Name,
        //                }).ToList();
        //    }
        //    else
        //    {
        //        return voucherRepository.GetAllVoucherTypes()
        //                .Select(a => new SelectListItem()
        //                {
        //                    Value = a.Id.ToString(),
        //                    Text = a.Name,
        //                }).ToList();
        //    }

        //}

    }
}