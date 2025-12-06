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
                throw new ArgumentException($"Origin airport '{criteria.Origin}' must be a European airport.");

            try
            {
                var token = await GetAccessTokenAsync();
                var duration = (criteria.ReturnDate - criteria.DepartureDate).Days;
                var maxPricePerPerson = (int)(criteria.MaxPrice / criteria.NumberOfPeople);
                if (criteria.Origin == "CLJ") criteria.Origin = "MAD";


                var flightUrl =
                    $"v1/shopping/flight-destinations?origin={criteria.Origin.ToUpper()}&departureDate={criteria.DepartureDate:yyyy-MM-dd}&maxPrice={maxPricePerPerson}&viewBy=DURATION";

                var request = new HttpRequestMessage(HttpMethod.Get, flightUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode) return null;

                var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
                if (!json.TryGetProperty("data", out var data) || data.GetArrayLength() == 0) return null;

                var allFlights = data.EnumerateArray().ToList();
                var random = new Random();

                allFlights = allFlights.OrderBy(x => random.Next()).ToList();

                foreach (var flight in allFlights)
                {
                    var destCode = flight.GetProperty("destination").GetString();
                    var flightPriceStr = flight.GetProperty("price").GetProperty("total").GetString();

                    if (string.IsNullOrEmpty(destCode) ||
                        !decimal.TryParse(flightPriceStr, out decimal flightPricePerPerson))
                        continue;

                    if (!AirportData.IsEuropeanAirport(destCode) || destCode == criteria.Origin) continue;

                    decimal totalFlightCost = flightPricePerPerson * criteria.NumberOfPeople;
                    decimal remainingBudget = criteria.MaxPrice - totalFlightCost;


                    if (remainingBudget < (30 * duration * criteria.NumberOfPeople)) continue;

                    var depDate = flight.GetProperty("departureDate").GetString();
                    var retDate = flight.GetProperty("returnDate").GetString();

                    var hotelOffer = await GetHotelForTrip(destCode, depDate, retDate, criteria.NumberOfPeople,
                        remainingBudget, token);

                    if (hotelOffer != null)
                    {
                        return new TripOption
                        {
                            Destination = AirportData.GetCityName(destCode),
                            DestinationCode = destCode,
                            FlightInfo = $"Flight: {depDate} to {retDate} (${flightPricePerPerson}/person)",
                            PricePerPerson =
                                flightPricePerPerson +
                                (hotelOffer.Price / criteria.NumberOfPeople), // Approx total per person
                            TotalPrice = totalFlightCost + hotelOffer.Price,
                            HotelInfo = $"{hotelOffer.HotelName} ({hotelOffer.Price} {hotelOffer.Currency} total)"
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        private async Task<HotelOffer?> GetHotelForTrip(string cityCode, string checkIn, string checkOut, int people,
            decimal maxBudget, string token)
        {
            try
            {
                var listUrl =
                    $"v1/reference-data/locations/hotels/by-city?cityCode={cityCode}&radius=10&radiusUnit=KM&hotelSource=ALL";

                var request = new HttpRequestMessage(HttpMethod.Get, listUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var listResponse = await _httpClient.SendAsync(request);

                if (!listResponse.IsSuccessStatusCode) return null;

                var listJson = JsonSerializer.Deserialize<JsonElement>(await listResponse.Content.ReadAsStringAsync());
                if (!listJson.TryGetProperty("data", out var hotelsData) || hotelsData.GetArrayLength() == 0)
                    return null;

                var hotelIds = hotelsData.EnumerateArray()
                    .Take(20)
                    .Select(h => h.GetProperty("hotelId").GetString())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                if (!hotelIds.Any()) return null;

                var idsString = string.Join(",", hotelIds);
                var offersUrl =
                    $"v3/shopping/hotel-offers?hotelIds={idsString}&adults={Math.Min(people, 9)}&checkInDate={checkIn}&checkOutDate={checkOut}&currency=EUR";

                var offerRequest = new HttpRequestMessage(HttpMethod.Get, offersUrl);
                offerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var offerResponse = await _httpClient.SendAsync(offerRequest);

                if (!offerResponse.IsSuccessStatusCode) return null;

                var offerJson =
                    JsonSerializer.Deserialize<JsonElement>(await offerResponse.Content.ReadAsStringAsync());
                if (!offerJson.TryGetProperty("data", out var offersData)) return null;

                foreach (var offer in offersData.EnumerateArray())
                {
                    if (offer.TryGetProperty("offers", out var rooms) && rooms.GetArrayLength() > 0)
                    {
                        var firstRoom = rooms[0];
                        var priceStr = firstRoom.GetProperty("price").GetProperty("total").GetString();
                        var currency = firstRoom.GetProperty("price").TryGetProperty("currency", out var curr)
                            ? curr.GetString()
                            : "EUR";

                        if (decimal.TryParse(priceStr, out decimal price) && price <= maxBudget)
                        {
                            var hotelName = offer.GetProperty("hotel").GetProperty("name").GetString();

                            return new HotelOffer
                            {
                                HotelName = hotelName,
                                Price = price,
                                Currency = currency
                            };
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }


        public IEnumerable<string> GetEuropeanAirports() => AirportData.EuropeanAirportCodes.OrderBy(x => x).ToList();
        public IEnumerable<AirportData.AirportInfo> GetEuropeanAirportsWithCities() => AirportData.GetAllAirports();
    }
}