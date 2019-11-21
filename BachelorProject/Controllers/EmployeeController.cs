using BachelorProject.Models;
using BachelorProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnviroment;
        private readonly ILogger logger;

        public EmployeeController(IEmployeeRepository employeeRepository,
                                  IHostingEnvironment hostingEnviroment,
                                  ILogger<EmployeeController> logger)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnviroment = hostingEnviroment;
            this.logger = logger;
        }

        [HttpGet]
        public ViewResult Index(string deleteMessage)
        {
            ViewBag.DeleteMessage = deleteMessage;

            var model = _employeeRepository.GetAllEmployees().OrderByDescending(e => e.IsValid);

            return View("~/Views/Employee/index.cshtml", model);
        }

        [HttpPost]
        public IActionResult DeleteEmployee(int id)
        {
            string deleteMessage = null;

            try
            {
                Employee employee = _employeeRepository.Delete(id);
                deleteMessage = "Zaměstnanec byl úspěšně smazán";
                return RedirectToAction("index", new { DeleteMessage = deleteMessage });
            }
            catch (DbUpdateException)
            {
                try
                {
                    Employee employee = _employeeRepository.GetEmployee(id);
                    employee.IsValid = false;
                    _employeeRepository.Update(employee);

                    deleteMessage = "Zaměstnanec memůže být smazán protože je využíván. " +
                                    "Zaměstnance nelze od teď využívat při vytváření nových přemluvených her. " +
                                    "Pokud chcete tohoto zaměstnance smazat, odstraňte přemluvené hry s tímto zaměstnancem a zkuste to znovu.";

                    return RedirectToAction("index", new { DeleteMessage = deleteMessage });
                }
                catch (Exception)
                {

                    deleteMessage = "Zaměstnance se nepodařilo smazat";
                    return RedirectToAction("index", new { DeleteMessage = deleteMessage });
                }
            }
        }

        [HttpGet]
        public ViewResult Details(int? id)
        {
            //throw new Exception("error in details view");
            //logger.LogTrace("Trace Log");
            //logger.LogDebug("Debug Log");
            //logger.LogInformation("Information Log");
            //logger.LogWarning("Warning Log");
            //logger.LogError("Error Log");
            //logger.LogCritical("Critical Log");

            Employee employee = _employeeRepository.GetEmployee(id.Value);

            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id.Value);
            }

            EmployeeDetailsViewModel employeeDetailsViewModel = new EmployeeDetailsViewModel()
            {
                Employee = employee,
                PageTitle = "Detail zaměstnance"
            };

            return View(employeeDetailsViewModel);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //string uniqueFileName = ProcessUploadedFile(model);

                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    //PhotoPath = uniqueFileName,
                    Role = model.Role,
                    IsValid = true
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("index");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);

            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Role = employee.Role,
                //ExistingPhotoPath = employee.PhotoPath,
                IsValid = employee.IsValid,
            };
            return View(employeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Role = model.Role;
                employee.IsValid = model.IsValid;

                if (model.Photo != null)
                {
                    if (model.ExistingPhotoPath != null)
                    {
                        string photoPath = Path.Combine(hostingEnviroment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(photoPath);
                    }
                    
                    //employee.PhotoPath = ProcessUploadedFile(model);
                }

                _employeeRepository.Update(employee);
                return RedirectToAction("index");
            }

            return View(model);
        }

        //private string ProcessUploadedFile(EmployeeCreateViewModel model)
        //{
        //    string uniqueFileName = null;
        //    if (model.Photo != null)
        //    {
        //        string uploadsFolder = Path.Combine(hostingEnviroment.WebRootPath, "images");
        //        uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            model.Photo.CopyTo(fileStream);
        //        };
                    
        //    }

        //    return uniqueFileName;
        //}

    }
}
