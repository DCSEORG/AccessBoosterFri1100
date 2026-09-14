using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AccessApp.Pages.Films;

public class EditModel : PageModel
{
    private readonly CinemaDb _db;
    public EditModel(CinemaDb db) => _db = db;

    [BindProperty] public int FilmId { get; set; }
    [BindProperty, Required, MaxLength(50)] public string FilmTitle { get; set; } = string.Empty;

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
        if (!ModelState.IsValid) return Page();
        _db.UpdateFilm(FilmId, FilmTitle.Trim());
        return RedirectToPage("Index");
    }
}
