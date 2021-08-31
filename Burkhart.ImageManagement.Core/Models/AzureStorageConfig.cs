using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Burkhart.ImageManagement.Core.Models
{
    public class AzureStorageConfig
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string ImageContainer { get; set; }
        public string ThumbnailContainer { get; set; }
        public string TenantId { get; set; }
        public string AppId { get; set; }
    }
}
