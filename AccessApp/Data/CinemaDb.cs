using Microsoft.Data.Sqlite;
using AccessApp.Models;

namespace AccessApp.Data;

/// <summary>
/// All database interactions are encapsulated here using parameterised commands.
/// No T-SQL is constructed from user input anywhere in the application.
/// These methods mirror the role of stored procedures, eliminating any SQL injection risk.
/// </summary>
public class CinemaDb
{
    private readonly string _connectionString;
    private const int TotalSeats = 50;

    public CinemaDb(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    // ─── Schema + Seed ────────────────────────────────────────────────────────

    public void InitialiseDatabase()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            PRAGMA journal_mode=WAL;

            CREATE TABLE IF NOT EXISTS Film (
                FilmId    INTEGER PRIMARY KEY AUTOINCREMENT,
                FilmTitle TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Customer (
                CustId   INTEGER PRIMARY KEY AUTOINCREMENT,
                CustName TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Showing (
                ShowingId   INTEGER PRIMARY KEY AUTOINCREMENT,
                FilmId      INTEGER NOT NULL REFERENCES Film(FilmId),
                ShowingDate TEXT NOT NULL,
                ShowingTime TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Booking (
                BookingId INTEGER PRIMARY KEY AUTOINCREMENT,
                ShowingId INTEGER NOT NULL REFERENCES Showing(ShowingId),
                CustId    INTEGER NOT NULL REFERENCES Customer(CustId),
                Seats     INTEGER NOT NULL CHECK(Seats > 0)
            );
        ";
        cmd.ExecuteNonQuery();
    }

    public void SeedData()
    {
        using var conn = OpenConnection();

        // Only seed when tables are empty
        using (var check = conn.CreateCommand())
        {
            check.CommandText = "SELECT COUNT(*) FROM Film";
            if ((long)(check.ExecuteScalar() ?? 0L) > 0) return;
        }

        using var tx = conn.BeginTransaction();

        var films = new[] { "The Dark Knight", "Inception", "Interstellar", "Tenet", "Oppenheimer" };
        foreach (var title in films)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO Film (FilmTitle) VALUES (@title)";
            cmd.Parameters.AddWithValue("@title", title);
            cmd.ExecuteNonQuery();
        }

        var customers = new[] { "Alice Smith", "Bob Jones", "Carol White", "David Brown", "Eve Taylor" };
        foreach (var name in customers)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO Customer (CustName) VALUES (@name)";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.ExecuteNonQuery();
        }

        // Showings: 3 future showings per film
        var dates = new[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(3), DateTime.Today.AddDays(7) };
        var times = new[] { "14:00", "17:30", "20:00" };

        for (int filmId = 1; filmId <= films.Length; filmId++)
        {
            for (int i = 0; i < dates.Length; i++)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO Showing (FilmId, ShowingDate, ShowingTime)
                    VALUES (@filmId, @date, @time)";
                cmd.Parameters.AddWithValue("@filmId", filmId);
                cmd.Parameters.AddWithValue("@date", dates[i].ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@time", times[i]);
                cmd.ExecuteNonQuery();
            }
        }

        // A few sample bookings
        var sampleBookings = new[] {
            (showingId: 1, custId: 1, seats: 2),
            (showingId: 1, custId: 2, seats: 3),
            (showingId: 2, custId: 3, seats: 1),
            (showingId: 4, custId: 4, seats: 4),
        };

        foreach (var b in sampleBookings)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO Booking (ShowingId, CustId, Seats)
                VALUES (@sid, @cid, @seats)";
            cmd.Parameters.AddWithValue("@sid", b.showingId);
            cmd.Parameters.AddWithValue("@cid", b.custId);
            cmd.Parameters.AddWithValue("@seats", b.seats);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
    }

    // ─── Films ────────────────────────────────────────────────────────────────

    public IList<Film> GetAllFilms()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT FilmId, FilmTitle FROM Film ORDER BY FilmTitle";
        using var reader = cmd.ExecuteReader();
        var list = new List<Film>();
        while (reader.Read())
            list.Add(new Film { FilmId = reader.GetInt32(0), FilmTitle = reader.GetString(1) });
        return list;
    }

    public Film? GetFilm(int filmId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT FilmId, FilmTitle FROM Film WHERE FilmId = @id";
        cmd.Parameters.AddWithValue("@id", filmId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? new Film { FilmId = reader.GetInt32(0), FilmTitle = reader.GetString(1) } : null;
    }

    public void InsertFilm(string filmTitle)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Film (FilmTitle) VALUES (@title)";
        cmd.Parameters.AddWithValue("@title", filmTitle);
        cmd.ExecuteNonQuery();
    }

    public void UpdateFilm(int filmId, string filmTitle)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Film SET FilmTitle = @title WHERE FilmId = @id";
        cmd.Parameters.AddWithValue("@title", filmTitle);
        cmd.Parameters.AddWithValue("@id", filmId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteFilm(int filmId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Film WHERE FilmId = @id";
        cmd.Parameters.AddWithValue("@id", filmId);
        cmd.ExecuteNonQuery();
    }

    // ─── Customers ────────────────────────────────────────────────────────────

    public IList<Customer> GetAllCustomers()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CustId, CustName FROM Customer ORDER BY CustName";
        using var reader = cmd.ExecuteReader();
        var list = new List<Customer>();
        while (reader.Read())
            list.Add(new Customer { CustId = reader.GetInt32(0), CustName = reader.GetString(1) });
        return list;
    }

    public Customer? GetCustomer(int custId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CustId, CustName FROM Customer WHERE CustId = @id";
        cmd.Parameters.AddWithValue("@id", custId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? new Customer { CustId = reader.GetInt32(0), CustName = reader.GetString(1) } : null;
    }

    public void InsertCustomer(string custName)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Customer (CustName) VALUES (@name)";
        cmd.Parameters.AddWithValue("@name", custName);
        cmd.ExecuteNonQuery();
    }

    public void UpdateCustomer(int custId, string custName)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Customer SET CustName = @name WHERE CustId = @id";
        cmd.Parameters.AddWithValue("@name", custName);
        cmd.Parameters.AddWithValue("@id", custId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteCustomer(int custId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Customer WHERE CustId = @id";
        cmd.Parameters.AddWithValue("@id", custId);
        cmd.ExecuteNonQuery();
    }

    // ─── Showings ─────────────────────────────────────────────────────────────

    public IList<Showing> GetUpcomingShowings()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime,
                   COALESCE(SUM(b.Seats), 0) AS SeatsBooked
            FROM Showing s
            INNER JOIN Film f ON f.FilmId = s.FilmId
            LEFT JOIN Booking b ON b.ShowingId = s.ShowingId
            WHERE date(s.ShowingDate) >= date('now')
            GROUP BY s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime
            ORDER BY f.FilmTitle, s.ShowingDate, s.ShowingTime";
        return ReadShowings(cmd);
    }

    public IList<Showing> GetAllShowings()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime,
                   COALESCE(SUM(b.Seats), 0) AS SeatsBooked
            FROM Showing s
            INNER JOIN Film f ON f.FilmId = s.FilmId
            LEFT JOIN Booking b ON b.ShowingId = s.ShowingId
            GROUP BY s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime
            ORDER BY s.ShowingDate, s.ShowingTime";
        return ReadShowings(cmd);
    }

    public Showing? GetShowing(int showingId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime,
                   COALESCE(SUM(b.Seats), 0) AS SeatsBooked
            FROM Showing s
            INNER JOIN Film f ON f.FilmId = s.FilmId
            LEFT JOIN Booking b ON b.ShowingId = s.ShowingId
            WHERE s.ShowingId = @id
            GROUP BY s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime";
        cmd.Parameters.AddWithValue("@id", showingId);
        return ReadShowings(cmd).FirstOrDefault();
    }

    private static IList<Showing> ReadShowings(SqliteCommand cmd)
    {
        using var reader = cmd.ExecuteReader();
        var list = new List<Showing>();
        while (reader.Read())
        {
            var dateStr = reader.GetString(3);
            var timeStr = reader.GetString(4);
            list.Add(new Showing
            {
                ShowingId   = reader.GetInt32(0),
                FilmId      = reader.GetInt32(1),
                FilmTitle   = reader.GetString(2),
                ShowingDate = DateTime.Parse(dateStr),
                ShowingTime = TimeSpan.Parse(timeStr),
                SeatsBooked = reader.GetInt32(5)
            });
        }
        return list;
    }

    public void InsertShowing(int filmId, DateTime date, TimeSpan time)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Showing (FilmId, ShowingDate, ShowingTime)
            VALUES (@filmId, @date, @time)";
        cmd.Parameters.AddWithValue("@filmId", filmId);
        cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@time", time.ToString(@"hh\:mm"));
        cmd.ExecuteNonQuery();
    }

    public void UpdateShowing(int showingId, int filmId, DateTime date, TimeSpan time)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Showing SET FilmId = @filmId, ShowingDate = @date, ShowingTime = @time
            WHERE ShowingId = @id";
        cmd.Parameters.AddWithValue("@filmId", filmId);
        cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@time", time.ToString(@"hh\:mm"));
        cmd.Parameters.AddWithValue("@id", showingId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteShowing(int showingId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Showing WHERE ShowingId = @id";
        cmd.Parameters.AddWithValue("@id", showingId);
        cmd.ExecuteNonQuery();
    }

    // ─── Bookings ─────────────────────────────────────────────────────────────

    public IList<Booking> GetAllBookings()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.BookingId, b.ShowingId, b.CustId, b.Seats,
                   f.FilmTitle, c.CustName, s.ShowingDate, s.ShowingTime
            FROM Booking b
            INNER JOIN Showing  s ON s.ShowingId = b.ShowingId
            INNER JOIN Film     f ON f.FilmId    = s.FilmId
            INNER JOIN Customer c ON c.CustId    = b.CustId
            ORDER BY s.ShowingDate, s.ShowingTime, c.CustName";
        return ReadBookings(cmd);
    }

    public Booking? GetBooking(int bookingId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT b.BookingId, b.ShowingId, b.CustId, b.Seats,
                   f.FilmTitle, c.CustName, s.ShowingDate, s.ShowingTime
            FROM Booking b
            INNER JOIN Showing  s ON s.ShowingId = b.ShowingId
            INNER JOIN Film     f ON f.FilmId    = s.FilmId
            INNER JOIN Customer c ON c.CustId    = b.CustId
            WHERE b.BookingId = @id";
        cmd.Parameters.AddWithValue("@id", bookingId);
        return ReadBookings(cmd).FirstOrDefault();
    }

    private static IList<Booking> ReadBookings(SqliteCommand cmd)
    {
        using var reader = cmd.ExecuteReader();
        var list = new List<Booking>();
        while (reader.Read())
        {
            list.Add(new Booking
            {
                BookingId   = reader.GetInt32(0),
                ShowingId   = reader.GetInt32(1),
                CustId      = reader.GetInt32(2),
                Seats       = reader.GetInt32(3),
                FilmTitle   = reader.GetString(4),
                CustName    = reader.GetString(5),
                ShowingDate = DateTime.Parse(reader.GetString(6)),
                ShowingTime = TimeSpan.Parse(reader.GetString(7))
            });
        }
        return list;
    }

    /// <summary>
    /// Returns seats still available for a given showing.
    /// </summary>
    public int GetSeatsAvailable(int showingId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COALESCE(SUM(Seats), 0) FROM Booking WHERE ShowingId = @id";
        cmd.Parameters.AddWithValue("@id", showingId);
        var booked = (long)(cmd.ExecuteScalar() ?? 0L);
        return TotalSeats - (int)booked;
    }

    /// <summary>
    /// Inserts a booking only if enough seats remain. Returns true on success.
    /// </summary>
    public (bool Success, string Message) InsertBooking(int showingId, int custId, int seats)
    {
        using var conn = OpenConnection();
        using var tx = conn.BeginTransaction();

        // Re-check seats inside the transaction to avoid races
        using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.Transaction = tx;
            checkCmd.CommandText = "SELECT COALESCE(SUM(Seats),0) FROM Booking WHERE ShowingId = @id";
            checkCmd.Parameters.AddWithValue("@id", showingId);
            var booked = (long)(checkCmd.ExecuteScalar() ?? 0L);
            int available = TotalSeats - (int)booked;
            if (seats > available)
            {
                tx.Rollback();
                return (false, $"Sorry – only {available} seat(s) are available for this showing.");
            }
        }

        using (var insCmd = conn.CreateCommand())
        {
            insCmd.Transaction = tx;
            insCmd.CommandText = @"
                INSERT INTO Booking (ShowingId, CustId, Seats)
                VALUES (@sid, @cid, @seats)";
            insCmd.Parameters.AddWithValue("@sid", showingId);
            insCmd.Parameters.AddWithValue("@cid", custId);
            insCmd.Parameters.AddWithValue("@seats", seats);
            insCmd.ExecuteNonQuery();
        }

        tx.Commit();
        return (true, "Booking accepted.");
    }

    public void DeleteBooking(int bookingId)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Booking WHERE BookingId = @id";
        cmd.Parameters.AddWithValue("@id", bookingId);
        cmd.ExecuteNonQuery();
    }
}
