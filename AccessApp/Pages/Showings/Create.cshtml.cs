using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccessApp.Pages.Showings;

public class CreateModel : PageModel
{
    private readonly CinemaDb _db;
    public CreateModel(CinemaDb db) => _db = db;

    [BindProperty] public int FilmId { get; set; }
    [BindProperty] public DateTime ShowingDate { get; set; } = DateTime.Today.AddDays(1);
    [BindProperty] public TimeSpan ShowingTime { get; set; } = new TimeSpan(14, 0, 0);
    public List<SelectListItem> FilmOptions { get; private set; } = [];

    public void OnGet() => LoadFilms();

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) { LoadFilms(); return Page(); }
        _db.InsertShowing(FilmId, ShowingDate, ShowingTime);
        return RedirectToPage("Index");
    }

    private void LoadFilms() =>
        FilmOptions = _db.GetAllFilms().Select(f => new SelectListItem(f.FilmTitle, f.FilmId.ToString())).ToList();
}
