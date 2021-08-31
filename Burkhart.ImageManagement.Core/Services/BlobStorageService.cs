using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Burkhart.ImageManagement.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Burkhart.ImageManagement.Core.Services
{
    public interface IStorageService
    {
        Task<List<string>> GetThumbNailUrls();
        Task<bool> UploadFileToStorage(MemoryStream fileStream, string fileName);

    }
    public class BlobStorageService
    {
        private readonly ILogger<BlobStorageService> logger;
        private readonly AzureStorageConfig _storageConfig;

        public BlobStorageService(ILogger<BlobStorageService> logger, AzureStorageConfig storageConfig)
        {
            this.logger = logger;
            this._storageConfig = storageConfig;

        }

        /// <summary>
        ///  Uploads a file image through a file stream   
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="fileName"></param>
        /// <param name="_storageConfig"></param>
        /// <returns></returns>
        public  async Task<bool> UploadFileToStorage(MemoryStream fileStream, string fileName)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _storageConfig.AccountName +
                                  ".blob.core.windows.net/" +
                                  _storageConfig.ImageContainer +
                                  "/" + fileName);

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            //reset the filestream position
            fileStream.Position = 0;

            // Upload the file
            await blobClient.UploadAsync(fileStream);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Retrieves thumbnail urls from a given storage account container    
        /// </summary>
        /// <param name="_storageConfig"></param>
        /// <returns></returns>
        public async Task<List<string>> GetThumbNailUrls()
        {
            List<string> thumbnailUrls = new List<string>();

            // Create a URI to the storage account
            Uri accountUri = new Uri("https://" + _storageConfig.AccountName + ".blob.core.windows.net/");

            // Create BlobServiceClient from the account URI
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountUri);

            // Get reference to the container
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(_storageConfig.ThumbnailContainer);

            if (container.Exists())
            {
                foreach (BlobItem blobItem in container.GetBlobs())
                {
                    thumbnailUrls.Add(container.Uri + "/" + blobItem.Name);
                }
            }

            return await Task.FromResult(thumbnailUrls);
        }

    }
}
