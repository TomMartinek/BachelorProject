using BachelorProject.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    /** 
     * Databázová entita "Uživatelé"
     * Tato třída rozšiřuje třídu "IdentityUser", která je poskytnuta ASP.Net Core frameworkem, o atribut "Pobočka"
     * **/

    public class ApplicationUser : IdentityUser
    {
        public BranchOfficeEnum? BranchOffice { get; set; }

    }
}
