using BachelorProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.ViewModels.AdditionalGameViewModels
{
    public class AdditionalGameIndexViewModel
    {
        public AdditionalGameIndexViewModel(){}

        public string SelectedMonth { get; set; }

        public IEnumerable<AdditionalGame> AdditionalGames { get; set; }

    }

}
