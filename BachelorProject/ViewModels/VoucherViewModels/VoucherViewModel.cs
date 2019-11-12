using BachelorProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.VoucherViewModels
{
    public class VoucherViewModel
    {
        public int Id { get; set; }

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
        public int Value { get; set; }

        [Required(ErrorMessage = "Vyberte typ voucheru")]
        [Display(Name = "Typ voucheru")]
        public int? VoucherTypeId { get; set; }

        public List<SelectListItem> VoucherTypes { get; set; }

        public string ApplicationUserName { get; set; }


    }
}
