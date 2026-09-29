using System.ComponentModel.DataAnnotations;
namespace ImageSizeConverterWeb.Models
{
    public class ImageResizeModel
    {
        [Required]
        public IFormFile ImageFile { get; set; }

    }
}
