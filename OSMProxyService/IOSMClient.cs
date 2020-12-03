using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSMProxyService
{
    public interface IOSMClient
    {
        Task<List<List<decimal[]>>> GetMultiPolygonsByLocationAsync(string location, int frequency);
    }
}
