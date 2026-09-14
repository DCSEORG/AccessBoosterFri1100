using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Films;

public class DeleteModel : PageModel
{
    private readonly CinemaDb _db;
    public DeleteModel(CinemaDb db) => _db = db;

    [BindProperty] public int FilmId { get; set; }
    public string FilmTitle { get; set; } = string.Empty;

    public IActionResult OnGet(int id)
    {
        var film = _db.GetFilm(id);
        if (film is null) return NotFound();
        FilmId = film.FilmId;
        FilmTitle = film.FilmTitle;
        return Page();
    }

    public IActionResult OnPost()
    {
        _db.DeleteFilm(FilmId);
        return RedirectToPage("Index");
    }
}
