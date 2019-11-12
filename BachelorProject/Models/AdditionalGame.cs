using BachelorProject.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public class AdditionalGame
    {
        public int Id { get; set; }

        //when the record was created
        public DateTime Date { get; set; }

        //how many games
        [Required(ErrorMessage = "Zadejte počet her")]
        [Display(Name = "Počet her")]
        [Range(1, 50)]
        public int Count { get; set; }

        //FK
        [Required(ErrorMessage = "Vyberte typ hry")]
        [Display(Name = "Typ hry")]
        public int GameTypeId { get; set; }
        public GameType GameType { get; set; }

        //FK
        [Display(Name = "Instruktor")]
        public int? InstructorId { get; set; }
        [InverseProperty("InstuctorAdditionalGames")]
        public virtual Employee Instructor { get; set; }

        //FK
        [Display(Name = "Barmanka")]
        public int? BarmaidId { get; set; }
        [InverseProperty("BarmaidAdditionalGames")]
        public virtual Employee Barmaid { get; set; }

        [ForeignKey("ApplicationUserId")]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public BranchOfficeEnum BranchOffice { get; set; }
    }
}
