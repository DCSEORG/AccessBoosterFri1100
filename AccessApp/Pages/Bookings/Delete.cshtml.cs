using AccessApp.Data;
using AccessApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Bookings;

public class DeleteModel : PageModel
{
    private readonly CinemaDb _db;
    public DeleteModel(CinemaDb db) => _db = db;

    [BindProperty] public int BookingId { get; set; }
    public Booking? Booking { get; private set; }

    public IActionResult OnGet(int id)
    {
        Booking = _db.GetBooking(id);
        if (Booking is null) return NotFound();
        BookingId = Booking.BookingId;
        return Page();
    }

    public IActionResult OnPost()
    {
        _db.DeleteBooking(BookingId);
        return RedirectToPage("Index", new { msg = "Booking cancelled.", ok = true });
    }
}
