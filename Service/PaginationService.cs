using SunAuto.Hateoas;
using SunAuto.Logging.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunAuto.Logging.Api
{
    public class PaginationService : IPaginationService
    {
        public List<Link> GeneratePaginationLinks(string baseUrl, string? continuationToken)
        {
            var links = new List<Link> { new Link(baseUrl, "self") };

            if (!string.IsNullOrEmpty(continuationToken))
            {
                links.Add(new Link($"{baseUrl}?next={continuationToken}", "next"));
            }

            return links;
        }
    }
}
