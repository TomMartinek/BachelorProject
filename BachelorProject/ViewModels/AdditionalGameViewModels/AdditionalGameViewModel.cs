using BachelorProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.AdditionalGameViewModels
{
    public class AdditionalGameViewModel
    {
        public int Id { get; set; }

        //when the record was created
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Zadejte datum")]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        //how many games
        [Required(ErrorMessage = "Zadejte počet her")]
        [Display(Name = "Počet her")]
        [Range(1, 50, ErrorMessage = "Počet her musí být v rozmezí od 1 do 50")]
        public int Count { get; set; }

        //FK
        [Required(ErrorMessage = "Vyberte typ hry")]
        [Display(Name = "Typ hry")]
        public int? GameTypeId { get; set; }

        //FK
        [Display(Name = "Instruktor")]
        public int? InstructorId { get; set; }

        //FK
        [Display(Name = "Barmanka")]
        public int? BarmaidId { get; set; }

        public List<SelectListItem> Barmaids { get; set; }
        public List<SelectListItem> Employees { get; set; }

        public List<SelectListItem> GameTypes { get; set; }

        public string ApplicationUserName { get; set; }


    }
}
