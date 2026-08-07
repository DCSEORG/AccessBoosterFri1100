using AccessApp.Data;
using AccessApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Bookings;

public class IndexModel : PageModel
{
    private readonly CinemaDb _db;
    public IList<Booking> Bookings { get; private set; } = [];
    public string? StatusMessage { get; private set; }
    public bool IsSuccess { get; private set; }

    public IndexModel(CinemaDb db) => _db = db;

    public void OnGet([FromQuery] string? msg, [FromQuery] bool ok = true)
    {
        Bookings = _db.GetAllBookings();
        StatusMessage = msg;
        IsSuccess = ok;
    }
}
