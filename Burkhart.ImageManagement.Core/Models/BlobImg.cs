using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Burkhart.BlobStorageService.Core.Models
{
    public class BlobImg
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; }
    }
}
