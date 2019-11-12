using BachelorProject.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.Identity
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Zadejte email")]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller: "Account")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Zadejte heslo")]
        [DataType(DataType.Password)]
        [Display(Name = "Heslo")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potvrďte heslo")]
        [Compare("Password", ErrorMessage = "Hesla nejsou shodná")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vyberte pobočku")]
        [Display(Name = "Pobočka")]
        public BranchOfficeEnum? BranchOffice { get; set; }



    }
}
