using AccessApp.Data;
using AccessApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Showings;

public class IndexModel : PageModel
{
    private readonly CinemaDb _db;
    public IList<Showing> Showings { get; private set; } = [];
    public IndexModel(CinemaDb db) => _db = db;
    public void OnGet() => Showings = _db.GetAllShowings();
}
