using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Carvisto.Services
{
    public interface IGoogleMapsService
    {
        Task<RouteInfo> GetRouteInfoAsync(string origin, string destination);
    }

    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoogleMapsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"];
        }

        public async Task<RouteInfo> GetRouteInfoAsync(string origin, string destination)
        {
            var requestUrl = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin}&destination={destination}&key={_apiKey}";

            var responce = await _httpClient.GetAsync(requestUrl);
            responce.EnsureSuccessStatusCode();
            
            var content = await responce.Content.ReadAsStringAsync();
            var directions = JsonSerializer.Deserialize<DirectionsResponce>(content);

            if (directions?.Routes?.Count > 0 && directions.Routes[0].Legs?.Count > 0)
            {
                var leg = directions.Routes[0].Legs[0];
                return new RouteInfo
                {
                    Distance = leg.Distance?.Text,
                    Duration = leg.Duration?.Text
                };
            }

            return new RouteInfo();
        }
    }
    
    public class RouteInfo
    {
        public string Distance { get; set; }
        public string Duration { get; set; }
    }
    
    // Class for answer API
    public class DirectionsResponce
    {
        public List<Route> Routes { get; set; }
    }

    public class Route
    {
        public List<Leg> Legs { get; set; }
    }

    public class Leg
    {
        public TextValue Distance { get; set; }
        public TextValue Duration { get; set; }
    }

    public class TextValue
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}