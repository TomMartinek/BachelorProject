using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.UserRole
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Zadejte název role")]
        [Display(Name = "Název role")]
        public string RoleName { get; set; }
    }
}
