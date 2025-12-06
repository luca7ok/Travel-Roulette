using PoliHack18.Models;
using PoliHack18.Constants;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PoliHack18.Services
{
    public class FlightService : IFlightService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private string? _accessToken;

        public FlightService(IConfiguration configuration)
        {
            _clientId = configuration["AmadeusApiKey"] ?? "";
            _clientSecret = configuration["AmadeusApiSecret"] ?? "";
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://test.api.amadeus.com/");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken)) return _accessToken;

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret)
                });


                var response = await _httpClient.PostAsync("v1/security/oauth2/token", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

                _accessToken = tokenResponse.GetProperty("access_token").GetString();
                return _accessToken ?? "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token Error: {ex.Message}");
                throw;
            }
        }

        public async Task<TripOption?> GetRandomFlight(TripSearchCriteria criteria)
        {
            if (!AirportData.IsEuropeanAirport(criteria.Origin))
            {
                throw new ArgumentException($"Origin airport '{criteria.Origin}' must be a European airport.");
            }
            try
            {
                
                var token = await GetAccessTokenAsync();
                var duration = (criteria.ReturnDate - criteria.DepartureDate).Days;
                
                var maxPricePerPerson = (int)(criteria.MaxPrice / criteria.NumberOfPeople);
                
                if (criteria.Origin == "CLJ")
                {
                    criteria.Origin = "MAD";
                }
                
                var url = $"v1/shopping/flight-destinations?origin={criteria.Origin.ToUpper()}&departureDate={criteria.DepartureDate:yyyy-MM-dd}&duration={duration}&maxPrice={maxPricePerPerson}&viewBy=DURATION";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode) return null;

                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var data = json.GetProperty("data");

                if (data.GetArrayLength() == 0) return null;

                var allFlights = data.EnumerateArray().ToList();

                var validFlights = allFlights
                    .Where(f =>
                    {
                        var dest = f.GetProperty("destination").GetString();
                        var priceStr = f.GetProperty("price").GetProperty("total").GetString();
                        
                        if (string.IsNullOrEmpty(dest) || !decimal.TryParse(priceStr, out decimal price)) 
                            return false;
                        
                        decimal totalFlightCost = price * criteria.NumberOfPeople;
                        decimal remainingBudget = criteria.MaxPrice - totalFlightCost;
                        decimal minimumDailyBudget = 30 * criteria.NumberOfPeople; 
                        
                        bool hasEnoughForHotel = remainingBudget >= (minimumDailyBudget * duration);

                        return AirportData.IsEuropeanAirport(dest) && 
                               dest != criteria.Origin &&
                               hasEnoughForHotel;
                    })
                    .ToList();

                if (validFlights.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No flights found that match criteria and leave budget for hotels.");
                    return null;
                }

                var random = new Random();
                
                var selectedFlight = validFlights[random.Next(validFlights.Count)];

                var destinationCode = selectedFlight.GetProperty("destination").GetString() ?? "";
                var priceVal = decimal.Parse(selectedFlight.GetProperty("price").GetProperty("total").GetString() ?? "0");
                var depDate = selectedFlight.GetProperty("departureDate").GetString();
                var retDate = selectedFlight.TryGetProperty("returnDate", out var ret) ? ret.GetString() : null;

                var destinationCity = AirportData.GetCityName(destinationCode);

                return new TripOption
                {
                    Destination = destinationCity,
                    DestinationCode = destinationCode,
                    FlightInfo = retDate != null
                        ? $"Departing {depDate}, Return {retDate}"
                        : $"Departing {depDate}",
                    PricePerPerson = priceVal,
                    TotalPrice = priceVal * criteria.NumberOfPeople
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<string> GetEuropeanAirports()
        {
            return AirportData.EuropeanAirportCodes.OrderBy(x => x).ToList();
        }

        public IEnumerable<AirportData.AirportInfo> GetEuropeanAirportsWithCities()
        {
            return AirportData.GetAllAirports();
        }
    }
}