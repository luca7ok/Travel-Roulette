namespace PoliHack18.Models;

public class TripOption
{
    public string Destination { get; set; } = string.Empty;
    public string? DestinationCode { get; set; } = string.Empty;
    public string FlightInfo { get; set; } = string.Empty;
    public decimal PricePerPerson { get; set; }
    public decimal TotalPrice { get; set; }
    public string HotelInfo { get; set; }
}