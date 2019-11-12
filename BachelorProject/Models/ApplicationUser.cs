using BachelorProject.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public BranchOfficeEnum? BranchOffice { get; set; }

    }
}
