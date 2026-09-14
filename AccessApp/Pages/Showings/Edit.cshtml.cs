using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccessApp.Pages.Showings;

public class EditModel : PageModel
{
    private readonly CinemaDb _db;
    public EditModel(CinemaDb db) => _db = db;

    [BindProperty] public int ShowingId { get; set; }
    [BindProperty] public int FilmId { get; set; }
    [BindProperty] public DateTime ShowingDate { get; set; }
    [BindProperty] public TimeSpan ShowingTime { get; set; }
    public List<SelectListItem> FilmOptions { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        var s = _db.GetShowing(id);
        if (s is null) return NotFound();
        ShowingId   = s.ShowingId;
        FilmId      = s.FilmId;
        ShowingDate = s.ShowingDate;
        ShowingTime = s.ShowingTime;
        LoadFilms();
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) { LoadFilms(); return Page(); }
        _db.UpdateShowing(ShowingId, FilmId, ShowingDate, ShowingTime);
        return RedirectToPage("Index");
    }

    private void LoadFilms() =>
        FilmOptions = _db.GetAllFilms().Select(f => new SelectListItem(f.FilmTitle, f.FilmId.ToString())).ToList();
}
