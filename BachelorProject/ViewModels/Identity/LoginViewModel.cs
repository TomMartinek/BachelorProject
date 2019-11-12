using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.Identity
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Zadejte email")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Zadejte heslo")]
        [DataType(DataType.Password)]
        [Display(Name = "Heslo")]
        public string Password { get; set; }

        [Display(Name = "Pamatuj si mě")]
        public bool RememberMe { get; set; }

    }
}
