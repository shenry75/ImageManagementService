using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Burkhart.ImageManagement.Core.Models;
using Burkhart.ImageManagement.Core.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using Microsoft.AspNetCore.Authorization;
using Burkhart.ImageManagement.Core.Services;

namespace Burkhart.ImageManagement.Api.Controllers
{
    //[Authorize(Policy = Burkhart.Utilities.DotNetCore.Authorization.PolicyConstants.DefaultPolicy)]
    [Route("ImageManagement")]
    [ApiController]
    public class ImageManagementController : ControllerBase
    {
        private readonly ILogger<ImageManagementController> _logger;
        private readonly AzureStorageConfig _storageConfig = null;
        private readonly IStorageService _service;

        public ImageManagementController(ILogger<ImageManagementController> logger, IOptions<AzureStorageConfig> config, IStorageService service)
        {
            _logger = logger;
            _storageConfig = config.Value;
            _service = service;
        }


        /// <summary>
        /// POST /api/images/upload
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if (_storageConfig.AccountKey == string.Empty || _storageConfig.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (_storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            //using (Stream stream = formFile.OpenReadStream())
                            //{
                                //resize
                                using (var img = Image.Load(formFile.OpenReadStream(), out IImageFormat format))
                                {


                                    string newSize = StorageHelper.ResizeImage(img, 178, 111);
                                    string[] aSize = newSize.Split(',');
                                    img.Mutate(h => h.Resize(Convert.ToInt32(aSize[1]), Convert.ToInt32(aSize[0])));

                                    //This section save the image to database using blob
                                    using (var ms = new MemoryStream())
                                    {
                                        img.Save(ms, format);
                                        isUploaded = await _service.UploadFileToStorage(ms, formFile.FileName);
                                        
                                    }


                                }
                                //resize end 
                                
                            //}
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    if (_storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();
                }
                else
                    return BadRequest("Look like the image couldnt upload to the storage");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// GET /api/images/thumbnails     
        /// </summary>
        /// <returns></returns>
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {
            try
            {
                if (_storageConfig.AccountKey == string.Empty || _storageConfig.AccountName == string.Empty)
                    return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

                if (_storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");

                List<string> thumbnailUrls = await _service.GetThumbNailUrls();
                return new ObjectResult(thumbnailUrls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}