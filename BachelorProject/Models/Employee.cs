using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vyplňte jméno a přijmení")]
        [MaxLength(50, ErrorMessage = "Jméno a příjmení nemůže mít více než 50 znaků")]
        [Display(Name = "Jméno a Příjmení")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vyplňte email")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", 
            ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; }

        public string PhotoPath { get; set; }

        public bool IsValid { get; set; }

        [Required(ErrorMessage = "Vyberte roli")]
        [Display(Name = "Role-Enum")]
        public EmployeeRoleEnum? Role { get; set; }

        public ICollection<AdditionalGame> InstuctorAdditionalGames { get; set; }
        public ICollection<AdditionalGame> BarmaidAdditionalGames { get; set; }
    }
}
