using PoliHack18.Models;
using PoliHack18.Constants;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using PoliHack18.Repository;

namespace PoliHack18.Services
{
    public class FlightService : IFlightService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private string? _accessToken;
        private DateTime _tokenExpiry;

        public FlightService(IConfiguration configuration)
        {
            _clientId = configuration["AmadeusApiKey"] ?? "";
            _clientSecret = configuration["AmadeusApiSecret"] ?? "";
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://test.api.amadeus.com/");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                return _accessToken;

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
                var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();
                _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);
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
                    var retDate = criteria.ReturnDate.ToString("yyyy-MM-dd");

                    var hotelOffer = await GetHotelForTrip(destCode, depDate, retDate, criteria.NumberOfPeople,
                        remainingBudget, token);

                    if (hotelOffer != null)
                    {
                        var offerJsonString = JsonSerializer.Serialize(flight);
                        Guid userID = UserSession.CurrentUserId;
                        decimal totalCost = totalFlightCost + hotelOffer.Price;
                        decimal commissionMultiplier = GetCommissionMultiplier(totalFlightCost + hotelOffer.Price);
                        decimal finalTotalPrice = totalCost * commissionMultiplier;

                        var trip = new TripOption
                        {
                            Destination = AirportData.GetCityName(destCode),
                            DestinationCode = destCode,
                            FlightInfo = $"{depDate} to {retDate} (${flightPricePerPerson}/person)",
                            PricePerPerson =
                                flightPricePerPerson +
                                (hotelOffer.Price / criteria.NumberOfPeople),
                            TotalPrice = finalTotalPrice,
                            HotelInfo = $"{hotelOffer.HotelName} ({hotelOffer.Price} {hotelOffer.Currency} total)",
                            FlightOfferJson = offerJsonString
                        };

                        try
                        {
                            var repo = new TripRepository();
                            repo.AddTrip(trip, userID);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine($"Could not save trip: {e.Message}");
                        }

                        return trip;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Class1.s = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error: ", ex.ToString(), "OK");
                });
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

        private async Task<JsonElement?> ConfirmFlightPrice(JsonElement flightOffer, string token)
        {
            var pricingUrl = "v1/shopping/flight-offers/pricing";
            var requestBody = new
            {
                data = new
                {
                    type = "flight-offers-pricing",
                    flightOffers = new[] { flightOffer }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var request = new HttpRequestMessage(HttpMethod.Post, pricingUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = jsonContent;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Pricing Error: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            if (json.TryGetProperty("data", out var data) &&
                data.TryGetProperty("flightOffers", out var validatedOffers) &&
                validatedOffers.GetArrayLength() > 0)
            {
                return validatedOffers[0];
            }

            return null;
        }

        public async Task<FlightBookingResult> CreateFlightOrder(TripOption trip, User user)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var flightOffer = JsonSerializer.Deserialize<JsonElement>(trip.FlightOfferJson);
                var confirmedOffer = await ConfirmFlightPrice(flightOffer, token);

                if (!confirmedOffer.HasValue)
                {
                    return new FlightBookingResult
                    {
                        Success = false,
                        Message = "Flight offer is no longer valid or price changed significantly.",
                        Trip = trip
                    };
                }

                var amadeusTravelers = new[]
                {
                    new
                    {
                        id = "1",
                        dateOfBirth = "1990-01-01",
                        name = new
                        {
                            firstName = user.Name ?? "John",
                            lastName = "Doe"
                        },
                        gender = "MALE",
                        contact = new
                        {
                            emailAddress = user.Email ?? "test@example.com",
                            phones = new[]
                            {
                                new
                                {
                                    deviceType = "MOBILE",
                                    countryCallingCode = "40",
                                    number = user.PhoneNumber ?? "123456789"
                                }
                            }
                        },
                        documents = new[]
                        {
                            new
                            {
                                documentType = "PASSPORT",
                                birthPlace = "Bucharest",
                                issuanceLocation = "RO",
                                issuanceDate = "2020-01-01",
                                number = "AB1234567",
                                expiryDate = "2030-01-01",
                                issuanceCountry = "RO",
                                validityCountry = "RO",
                                nationality = "RO",
                                holder = true
                            }
                        }
                    }
                };

                var contactInfo = new
                {
                    addresseeName = new
                    {
                        firstName = user.Name ?? "John",
                        lastName = "Doe"
                    },
                    companyName = "TRAVELER",
                    purpose = "STANDARD",
                    phones = new[]
                    {
                        new
                        {
                            deviceType = "MOBILE",
                            countryCallingCode = "40",
                            number = user.PhoneNumber ?? "123456789"
                        }
                    },
                    emailAddress = user.Email ?? "test@example.com",
                    address = new
                    {
                        lines = new[] { "123 Main Street" },
                        postalCode = "400000",
                        cityName = "Cluj-Napoca",
                        countryCode = "RO"
                    }
                };

                var bookingUrl = "v1/booking/flight-orders";
                var requestBody = new
                {
                    data = new
                    {
                        type = "flight-order",
                        flightOffers = new[] { confirmedOffer.Value },
                        travelers = amadeusTravelers,
                        remarks = new
                        {
                            general = new[]
                            {
                                new { subType = "GENERAL_MISCELLANEOUS", text = "ONLINE BOOKING" }
                            }
                        },
                        ticketingAgreement = new
                        {
                            option = "DELAY_TO_CANCEL",
                            delay = "6D"
                        },
                        contacts = new[] { contactInfo }
                    }
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var request = new HttpRequestMessage(HttpMethod.Post, bookingUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = jsonContent;

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Booking Error: {responseContent}");
                    return new FlightBookingResult
                    {
                        Success = false,
                        Message = $"Booking failed: {responseContent}",
                        Trip = trip,
                        RawResponse = responseContent
                    };
                }

                var json = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (json.TryGetProperty("data", out var data))
                {
                    var result = new FlightBookingResult
                    {
                        Success = true,
                        Message = "Booking successful!",
                        Trip = trip,
                        BookingDate = DateTime.UtcNow,
                        RawResponse = responseContent
                    };

                    if (data.TryGetProperty("associatedRecords", out var records) && records.GetArrayLength() > 0)
                    {
                        result.BookingReference = records[0].GetProperty("reference").GetString();
                    }

                    if (data.TryGetProperty("id", out var orderId))
                    {
                        result.OrderId = orderId.GetString();
                    }

                    try
                    {
                        var repo = new TripRepository();
                        // repo.SaveBooking(UserSession.CurrentUserId, result.BookingReference, result.OrderId, trip);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not save booking: {e.Message}");
                    }

                    return result;
                }

                return new FlightBookingResult
                {
                    Success = false,
                    Message = "Booking response was invalid",
                    Trip = trip,
                    RawResponse = responseContent
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Booking Exception: {ex.Message}");
                return new FlightBookingResult
                {
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Trip = trip
                };
            }
        }

        private decimal GetCommissionMultiplier(decimal totalPrice)
        {
            if (totalPrice <= 1000) return 1.05m;
            if (totalPrice <= 1750) return 1.04m;
            return 1.03m;
        }

        public IEnumerable<string> GetEuropeanAirports() => AirportData.EuropeanAirportCodes.OrderBy(x => x).ToList();
        public IEnumerable<AirportData.AirportInfo> GetEuropeanAirportsWithCities() => AirportData.GetAllAirports();
    }
}