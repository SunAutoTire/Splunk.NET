using Microsoft.Extensions.Logging;
using SunAuto.Logging.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunAuto.Logging.Client.StorageService
{
    public class EntryUpdateRequest : Entry
    {
        public int EventId { get; set; }
        public string? EventName { get; set; }

    }
}
