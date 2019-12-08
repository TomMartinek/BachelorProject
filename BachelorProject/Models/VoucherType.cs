using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    // TATO ENTITA ZATÍM NENÍ V APLIKACI NIKDE VYUŽÍVÁNA, JE ZDE PRO ÚČELY BUDOUCÍHO VÝVOJE
    public class VoucherType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vyplňte název")]
        [Display(Name = "Název")]
        [MaxLength(50, ErrorMessage = "Název nemůže být delší než 50 znaků")]
        public string Name { get; set; }

        public ICollection<Voucher> Vouchers { get; set; }

        [Display(Name = "Je platný")]
        public bool IsValid { get; set; }


    }
}
