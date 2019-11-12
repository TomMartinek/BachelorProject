using BachelorProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Vyplňte jméno a přijmení")]
        [MaxLength(50, ErrorMessage = "Jméno a příjmení nemůže mít více než 50 znaků")]
        [Display(Name = "Jméno a Příjmení")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vyplňte email")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
            ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; }

        public IFormFile Photo { get; set; }

        [Required(ErrorMessage = "Vyberte roli")]
        [Display(Name = "Role-Enum")]
        public EmployeeRoleEnum? Role { get; set; }
    }
}
