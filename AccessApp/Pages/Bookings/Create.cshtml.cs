using AccessApp.Data;
using AccessApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccessApp.Pages.Bookings;

public class CreateModel : PageModel
{
    private readonly CinemaDb _db;
    public CreateModel(CinemaDb db) => _db = db;

    [BindProperty] public int ShowingId { get; set; }
    [BindProperty] public int CustId { get; set; }
    [BindProperty] public int Seats { get; set; } = 1;

    public IList<Showing> Showings { get; private set; } = [];
    public List<SelectListItem> CustomerOptions { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public void OnGet() => LoadLookups();

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) { LoadLookups(); return Page(); }

        var (success, message) = _db.InsertBooking(ShowingId, CustId, Seats);
        if (!success)
        {
            ErrorMessage = message;
            LoadLookups();
            return Page();
        }

        return RedirectToPage("Index", new { msg = message, ok = true });
    }

    private void LoadLookups()
    {
        Showings = _db.GetUpcomingShowings();
        CustomerOptions = _db.GetAllCustomers()
            .Select(c => new SelectListItem(c.CustName, c.CustId.ToString()))
            .ToList();
    }
}
