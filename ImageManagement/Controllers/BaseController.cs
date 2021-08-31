using Burkhart.ImageManagement.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Burkhart.ImageManagement.Api.Controllers
{
    public class BaseController<TOutputContract> : Burkhart.Utilities.DotNetCore.Controllers.BaseController
    {
        private readonly IBaseService<TOutputContract> service;

        protected BaseController(IBaseService<TOutputContract> service)
        {
            this.service = service;
        }

        [HttpGet("{key}")]
        protected async Task<ActionResult<TOutputContract>> Get(string key)
        {
            var result = await this.service.Get(key);

            return GetResult(result);
        }

    }
}
