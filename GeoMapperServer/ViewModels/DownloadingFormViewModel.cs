using System.ComponentModel.DataAnnotations;

namespace GeoMapper.ViewModels
{
    public class DownloadingFormViewModel
    {
        [Required(ErrorMessage = "Enter the file name")]
        public string FileName { get; set; }
        public string MapBase64Img { get; set; }
    }
}
