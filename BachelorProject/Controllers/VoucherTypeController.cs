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
    public class VoucherTypeController : Controller
    {
        private readonly IVoucherTypeRepository voucherTypeRepository;

        public VoucherTypeController(IVoucherTypeRepository voucherTypeRepository)
        {
            this.voucherTypeRepository = voucherTypeRepository;
        }

        [HttpGet]
        public ViewResult Index(string deleteMessage)
        {
            ViewBag.DeleteMessage = deleteMessage;
            var model = voucherTypeRepository.GetAllVoucherTypes().OrderByDescending(vt => vt.IsValid);

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteVoucherType(int id)
        {
            string deleteMessage = null;

            try
            {
                VoucherType voucherType = voucherTypeRepository.Delete(id);
                deleteMessage = "Typ voucheru úspěšně smazán";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage });
            }
            catch (DbUpdateException)
            {
                try
                {
                    VoucherType voucherType = voucherTypeRepository.GetVoucherType(id);
                    voucherType.IsValid = false;
                    voucherTypeRepository.Update(voucherType);

                    deleteMessage = "Typ voucheru memůže být smazán protože je využíván. " +
                                    "Typ voucheru nelze od teď využívat při vytváření nových voucherů. " +
                                    "Pokud chcete tento typ voucheru smazat, odstraňte vouchery s tímto typem a zkuste to znovu.";

                    return RedirectToAction("index", new { DeleteMessage = deleteMessage });
                }
                catch (DbUpdateException)
                {
                    deleteMessage = "Typ voucheru se nepodařilo smazat";
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
        public IActionResult Create(VoucherType model)
        {
            if (ModelState.IsValid)
            {
                VoucherType newVoucherType = new VoucherType
                {
                    Name = model.Name,
                    IsValid = true,
                };

                voucherTypeRepository.Add(newVoucherType);
                return RedirectToAction("index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            VoucherType voucherType = voucherTypeRepository.GetVoucherType(id);
            VoucherType voucherTypeEditModel = new VoucherType
            {
                Id = voucherType.Id,
                Name = voucherType.Name,
                IsValid = voucherType.IsValid
            };
            return View(voucherTypeEditModel);
        }

        [HttpPost]
        public IActionResult Edit(VoucherType model)
        {
            if (ModelState.IsValid)
            {
                VoucherType voucherType = voucherTypeRepository.GetVoucherType(model.Id);
                voucherType.Name = model.Name;
                voucherType.IsValid = model.IsValid;

                voucherTypeRepository.Update(voucherType);
                return RedirectToAction("index");
            }

            return View();
        }
    }
}