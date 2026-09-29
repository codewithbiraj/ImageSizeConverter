using Microsoft.AspNetCore.Mvc;
using ImageSizeConverterWeb.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageSizeConverterWeb.Controllers
{
    public class ImageController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ImageController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // SHOW PAGE
        [HttpGet]
        public IActionResult Index()
        {
            // IMPORTANT: Explicitly point to Home/Index.cshtml
            return View("~/Views/Home/Index.cshtml");
        }

        // RESIZE IMAGE
        [HttpPost]
        public IActionResult Resize(ImageResizeModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
                return View("~/Views/Home/Index.cshtml");

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            var originalFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
            var originalPath = Path.Combine(uploads, originalFileName);

            // SAVE ORIGINAL
            using (var stream = new FileStream(originalPath, FileMode.Create))
            {
                model.ImageFile.CopyTo(stream);
            }

            double originalSizeKB = new FileInfo(originalPath).Length / 1024d;

            // LOAD IMAGE
            //The original image is loaded from the server.
            //The image is resized to half its original dimensions.
            //A new bitmap image is created using smaller dimensions.
            using var image = System.Drawing.Image.FromFile(originalPath);

            int newWidth = image.Width / 2;
            int newHeight = image.Height / 2;

            using var resizedBitmap = new Bitmap(image, new Size(newWidth, newHeight));

            var resizedFileName = "resized_" + originalFileName;
            var resizedPath = Path.Combine(uploads, resizedFileName);
            // SAVE RESIZED WITH QUALITY 50
            // The resized image is saved to the server with a quality setting of 50.
            // An encoder is obtained for JPEG format, and encoder parameters are set to specify the quality.
            // The resized bitmap is then saved using the specified encoder and parameters.
            // Finally, the new size of the resized image is calculated.
            // The relevant information about the original and resized images is prepared for display in the view.

            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] =
                new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

            resizedBitmap.Save(resizedPath, encoder, encoderParams);
            // image size formula
            //File size is read in bytes.
            // It is then divided by 1024 to convert it to kilobytes (KB).
            

            double newSizeKB = new FileInfo(resizedPath).Length / 1024d;

            ViewBag.ImagePath = "/uploads/" + resizedFileName;
            ViewBag.FileName = resizedFileName;
            ViewBag.OriginalSize = originalSizeKB;
            ViewBag.NewSize = newSizeKB;
            ViewBag.ReducedKB = originalSizeKB - newSizeKB;
            ViewBag.ReducedPercent =
                ((originalSizeKB - newSizeKB) / originalSizeKB) * 100;

            return View("~/Views/Home/Index.cshtml");
        }

        // DOWNLOAD
        public IActionResult Download(string fileName)
        {
            var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
            return PhysicalFile(path, "image/jpeg", fileName);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders()
                .First(c => c.FormatID == format.Guid);
        }
    }
}
