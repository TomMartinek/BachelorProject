using BachelorProject.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.User
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Roles = new List<string>();
        }

        public string Id { get; set; }

        //[Required(ErrorMessage = "Uživatelské jméno")]
        //[Display(Name = "Uživatelské jméno")]
        //public string UserName { get; set; }

        [Required(ErrorMessage = "Zadejte email")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vyberte pobočku")]
        [Display(Name = "Pobočka")]
        public BranchOfficeEnum? BranchOffice { get; set; }

        public IList<string> Roles { get; set; }

    }
}
