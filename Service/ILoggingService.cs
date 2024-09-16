using SunAuto.Hateoas;
using SunAuto.Logging.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunAuto.Logging.Api
{
    public interface ILoggingService
    {
        Task<Linked<IEnumerable<Entry>>> ListAsync(string? next, string? application, string? level, DateTimeOffset? startDate,
            DateTimeOffset? endDate, CancellationToken cancellationToken);
        Task<string> ExportLogsAsync(string? application, string? level, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken);
    }
}