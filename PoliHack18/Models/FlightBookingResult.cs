namespace PoliHack18.Models;

public class FlightBookingResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string BookingReference { get; set; }
    public string OrderId { get; set; }
    public TripOption Trip { get; set; }    
    public DateTime BookingDate { get; set; }
    public string RawResponse { get; set; }
}