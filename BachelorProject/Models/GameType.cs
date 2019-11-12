using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public class GameType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vyplňte název")]
        [Display(Name = "Název")]
        [MaxLength(50, ErrorMessage = "Název nemůže mít více než 50 znaků")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vyplňte cenu hry")]
        [Display(Name = "Cena hry")]
        [Range(1, 999.99, ErrorMessage = "Cena hry musí být kladné číslo")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Vyplňte provizi pro hlavní barmanku")]
        [Display(Name = "Provize pro hlavní barmanku")]
        [Range(1, 999.99, ErrorMessage = "Provize musí být kladné číslo")]
        public int BarmaidCommision { get; set; }

        [Required(ErrorMessage = "Vyplňte provizi pro instruktora")]
        [Display(Name = "Provize pro instruktora")]
        [Range(1, 999.99, ErrorMessage = "Provize musí být kladné číslo")]
        public int InstruktorCommision { get; set; }

        [Required(ErrorMessage = "Vyplňte provizi pro sólo přemluvenou hru")]
        [Display(Name = "Provize pro sólo přemluvenou hru")]
        [Range(1, 999.99, ErrorMessage = "Provize musí být kladné číslo")]
        public int SoloCommision { get; set; }

        public ICollection<AdditionalGame> AdditionalGames { get; set; }

        [Display(Name = "Je platný")]
        public bool IsValid { get; set; }


    }
}
