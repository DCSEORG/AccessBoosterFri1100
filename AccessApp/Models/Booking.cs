namespace AccessApp.Models;

public class Booking
{
    public int BookingId { get; set; }
    public int ShowingId { get; set; }
    public int CustId { get; set; }
    public int Seats { get; set; }
    public string FilmTitle { get; set; } = string.Empty;
    public string CustName { get; set; } = string.Empty;
    public DateTime ShowingDate { get; set; }
    public TimeSpan ShowingTime { get; set; }
}
