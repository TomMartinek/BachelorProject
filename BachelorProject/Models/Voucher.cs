using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public class Voucher
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; }

        public string Code { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Vyplňte datum začátku platnosti")]
        [Display(Name = "Platný od")]
        public DateTime ValidFrom { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Vyplňte datum konce platnosti")]
        [Display(Name = "Platný do")]
        public DateTime ValidUntil { get; set; }

        [Required(ErrorMessage = "Vyplňte titulek voucheru")]
        [Display(Name = "Titulek")]
        [MaxLength(50, ErrorMessage = "Popis voucheru nemůže být delší než 50 znaků")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vyplňte popis voucheru")]
        [Display(Name = "Popis")]
        [MaxLength(250, ErrorMessage = "Popis voucheru nemůže být delší než 250 znaků")]
        public string Description { get; set; }

        [Range(1, 5000, ErrorMessage = "Hodnota voucheru musí být v rozmezí od 1 do 5000")]
        [Display(Name = "Hodnota")]
        [Required(ErrorMessage = "Zadejte hodnotu")]
        public int Value { get; set; }

        public bool IsValid { get; set; }

        [Required(ErrorMessage = "Vyberte typ voucheru")]
        [Display(Name = "Typ voucheru")]
        public int VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public VoucherType VoucherType { get; set; }

        [ForeignKey("ApplicationUserId")]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
