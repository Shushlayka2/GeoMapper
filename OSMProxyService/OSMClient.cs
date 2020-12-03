using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace OSMProxyService
{
    public class OSMClient : IOSMClient
    {
        protected ILogger<OSMClient> Logger { get; }
        protected IHttpClientFactory ClientFactory { get; }
        protected IConfiguration Configuration { get; }

        public OSMClient(IHttpClientFactory clientFactory, ILogger<OSMClient> logger)
        {
            ClientFactory = clientFactory;
            Logger = logger;

            var builder = new ConfigurationBuilder().AddJsonFile("appconfig.json");
            Configuration = builder.Build();
        }

        public async Task<List<List<decimal[]>>> GetMultiPolygonsByLocationAsync(string location, int frequency)
        {
            try
            {
                var osm_ids = await GetOsmIds(location);
                var multiPolygons = new List<List<decimal[]>>();
                foreach (var osm_id in osm_ids)
                {
                    var multiPolygon = await GetMultiPolygon(osm_id, frequency);
                    if (multiPolygon != null)
                        multiPolygons.AddRange(multiPolygon);
                }
                return multiPolygons;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }
        }

        protected string CreateUrl(string hostName, object _params = null)
        {
            var host = Configuration.GetSection("AppSettings").GetValue<string>(hostName);
            var builder = new UriBuilder(host);
            var query = HttpUtility.ParseQueryString(builder.Query);
            _params.GetType().GetProperties().ToList()
            .ForEach(pi => query.Add(pi.Name, pi.GetValue(_params, null).ToString()));
            builder.Query = query.ToString();
            return builder.ToString();
        }

        protected async Task<IEnumerable<long>> GetOsmIds(string location)
        {
            var url = CreateUrl("NominatimHost", new { q = location, format = "geojson" });
            var response = await SendHttpGetRequest(url);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var jsonObj = JObject.Parse(responseJson);
                var ids = from feature in jsonObj["features"]
                          select (long)feature["properties"]["osm_id"];
                return ids;
            }
            else
            {
                throw new Exception("Nominatim search API doesn't response!");
            }
        }

        protected async Task<List<List<decimal[]>>> GetMultiPolygon(long osm_id, int frequency)
        {
            var url = CreateUrl("PolygonsHost", new { id = osm_id });
            await SendHttpGetRequest(url);

            url = CreateUrl("PolygonsHostGeoJson", new { id = osm_id });
            var response = await SendHttpGetRequest(url);
            if (response.IsSuccessStatusCode)
            {
                var multiPolygon = new List<List<decimal[]>>();
                var responseJson = await response.Content.ReadAsStringAsync();
                responseJson = responseJson.TrimEnd();
                if (responseJson != "None")
                {
                    var jsonObj = JObject.Parse(responseJson);
                    var multiPolygonJson = jsonObj["geometries"][0]["coordinates"];
                    var polygons = from polygon_outer in multiPolygonJson
                                   from polygon_inner in polygon_outer
                                   select polygon_inner;
                    foreach (var polygon in polygons)
                    {
                        var points = (from coordinate in polygon.Where((coordinate, index) => index % frequency == 0)
                                      select new decimal[2] { (decimal)coordinate[1], (decimal)coordinate[0] }).ToList();
                        multiPolygon.Add(points);
                    }
                    return multiPolygon;
                }
                return null;
            }
            else
            {
                throw new Exception("OSM Polygons API doesn't response!");
            }
        }

        protected async Task<HttpResponseMessage> SendHttpGetRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "OSMClient");
            var client = ClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            return response;
        }
    }
}
