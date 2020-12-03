using GeoMapper.Models.Validators;
using System.ComponentModel.DataAnnotations;

namespace GeoMapper.ViewModels
{
    public class SearchingFormViewModel
    {
        [Required(ErrorMessage = "Enter the location")]
        public string Location { get; set; }

        [PositiveValue(ErrorMessage = "Frequency has to be positive")]
        public int Frequency { get; set; } = 1;
    }
}
