using AccessApp.Data;
using AccessApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccessApp.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly CinemaDb _db;
    public IList<Customer> Customers { get; private set; } = [];
    public IndexModel(CinemaDb db) => _db = db;
    public void OnGet() => Customers = _db.GetAllCustomers();
}
