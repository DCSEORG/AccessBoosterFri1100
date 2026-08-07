using AccessApp.Data;
using AccessApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Films;

public class IndexModel : PageModel
{
    private readonly CinemaDb _db;
    public IList<Film> Films { get; private set; } = [];

    public IndexModel(CinemaDb db) => _db = db;

    public void OnGet() => Films = _db.GetAllFilms();
}
