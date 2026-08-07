using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Customers;

public class DeleteModel : PageModel
{
    private readonly CinemaDb _db;
    public DeleteModel(CinemaDb db) => _db = db;

    [BindProperty] public int CustId { get; set; }
    public string CustName { get; set; } = string.Empty;

    public IActionResult OnGet(int id)
    {
        var c = _db.GetCustomer(id);
        if (c is null) return NotFound();
        CustId = c.CustId;
        CustName = c.CustName;
        return Page();
    }

    public IActionResult OnPost()
    {
        _db.DeleteCustomer(CustId);
        return RedirectToPage("Index");
    }
}
