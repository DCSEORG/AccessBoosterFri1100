using AccessApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AccessApp.Pages.Customers;

public class CreateModel : PageModel
{
    private readonly CinemaDb _db;
    public CreateModel(CinemaDb db) => _db = db;

    [BindProperty, Required, MaxLength(50)]
    public string CustName { get; set; } = string.Empty;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        _db.InsertCustomer(CustName.Trim());
        return RedirectToPage("Index");
    }
}
