using GeoMapper.Models;
using GeoMapper.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSMProxyService;
using System;
using System.Threading.Tasks;

namespace GeoMapper.Controllers
{
    public class HomeController : Controller
    {
        protected ILogger<HomeController> Logger { get; }
        protected IOSMClient OMSClient { get; }

        public HomeController(ILogger<HomeController> logger, IOSMClient client)
        {
            Logger = logger;
            OMSClient = client;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FindLocation(SearchingFormViewModel searchingForm)
        {
            if (!ModelState.IsValid)
                return Json(new ResponseMessage(Status.Fail, null, ModelState));

            var multiPolygon = await OMSClient.GetMultiPolygonsByLocationAsync(searchingForm.Location, searchingForm.Frequency);

            if (multiPolygon == null)
                return Json(new ResponseMessage(Status.Fail));
            else
                return Json(new ResponseMessage(Status.Success, multiPolygon));
        }

        [HttpPost]
        public IActionResult UploadFile(DownloadingFormViewModel downloadingForm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                downloadingForm.MapBase64Img = downloadingForm.MapBase64Img.Replace("data:image/png;base64,", "");
                var bytes = Convert.FromBase64String(downloadingForm.MapBase64Img);
                return File(bytes, "image/png", downloadingForm.FileName + ".png");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return BadRequest();
            }
        }
    }
}
