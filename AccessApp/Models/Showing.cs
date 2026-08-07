namespace AccessApp.Models;

public class Showing
{
    public int ShowingId { get; set; }
    public int FilmId { get; set; }
    public DateTime ShowingDate { get; set; }
    public TimeSpan ShowingTime { get; set; }
    public string FilmTitle { get; set; } = string.Empty;
    public int SeatsBooked { get; set; }
    public int SeatsLeft => 50 - SeatsBooked;
}
