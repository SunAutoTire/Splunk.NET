using SunAuto.Hateoas;

namespace SunAuto.Logging.Api.Services
{
    public interface IPaginationService
    {
        List<Link> GeneratePaginationLinks(string baseUrl, string? continuationToken);
    }
}