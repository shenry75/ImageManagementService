using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Burkhart.ImageManagement.Core.Services
{
    public interface IBaseService<TOutputContract>
    {
        Task<TOutputContract> Get(string key);
    }

    public class BaseService<Tservice>
    {
        private readonly ILogger<Tservice> logger;

        public BaseService(ILogger<Tservice> logger)
        {
            this.logger = logger;
        }

        protected void ValidateParameters(Func<bool> invalidCheckFunction, string logMessage, string exceptionMessage = null)
        {
            if (invalidCheckFunction())
            {
                this.logger.LogError(logMessage);

                var exception = string.IsNullOrEmpty(exceptionMessage) ? new ArgumentNullException()
                    : new ArgumentNullException(exceptionMessage);

                throw exception;
            }
        }

    }
}
