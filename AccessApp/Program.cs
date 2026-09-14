using AccessApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=/home/app.db";
builder.Services.AddSingleton(_ => new CinemaDb(connectionString));

var app = builder.Build();

// Initialise and seed the database
try
{
    var db = app.Services.GetRequiredService<CinemaDb>();
    db.InitialiseDatabase();
    db.SeedData();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database initialisation failed. The application will continue to start.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
