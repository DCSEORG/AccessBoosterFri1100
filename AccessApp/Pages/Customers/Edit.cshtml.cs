using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AccessApp.Pages.Customers;

public class EditModel : PageModel
{
    private readonly CinemaDb _db;
    public EditModel(CinemaDb db) => _db = db;

    [BindProperty] public int CustId { get; set; }
    [BindProperty, Required, MaxLength(50)] public string CustName { get; set; } = string.Empty;

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
        if (!ModelState.IsValid) return Page();
        _db.UpdateCustomer(CustId, CustName.Trim());
        return RedirectToPage("Index");
    }
}
