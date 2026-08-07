using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Showings;

public class DeleteModel : PageModel
{
    private readonly CinemaDb _db;
    public DeleteModel(CinemaDb db) => _db = db;

    [BindProperty] public int ShowingId { get; set; }
    public string FilmTitle { get; set; } = string.Empty;
    public DateTime ShowingDate { get; set; }
    public TimeSpan ShowingTime { get; set; }

    public IActionResult OnGet(int id)
    {
        var s = _db.GetShowing(id);
        if (s is null) return NotFound();
        ShowingId   = s.ShowingId;
        FilmTitle   = s.FilmTitle;
        ShowingDate = s.ShowingDate;
        ShowingTime = s.ShowingTime;
        return Page();
    }

    public IActionResult OnPost()
    {
        _db.DeleteShowing(ShowingId);
        return RedirectToPage("Index");
    }
}
