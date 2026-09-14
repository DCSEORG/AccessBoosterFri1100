-- ============================================================================
-- Cinema Booking – Azure SQL Database Schema & Stored Procedures
-- Run this script against the Azure SQL Database after deploying sql.bicep
-- ============================================================================

-- ─── Tables ──────────────────────────────────────────────────────────────

IF OBJECT_ID('dbo.Booking', 'U')  IS NULL
CREATE TABLE dbo.Booking (
    BookingId  INT IDENTITY(1,1) PRIMARY KEY,
    ShowingId  INT NOT NULL,
    CustId     INT NOT NULL,
    Seats      SMALLINT NOT NULL CHECK (Seats > 0)
);

IF OBJECT_ID('dbo.Customer', 'U') IS NULL
CREATE TABLE dbo.Customer (
    CustId   INT IDENTITY(1,1) PRIMARY KEY,
    CustName NVARCHAR(50) NOT NULL
);

IF OBJECT_ID('dbo.Film', 'U') IS NULL
CREATE TABLE dbo.Film (
    FilmId    INT IDENTITY(1,1) PRIMARY KEY,
    FilmTitle NVARCHAR(50) NOT NULL
);

IF OBJECT_ID('dbo.Showing', 'U') IS NULL
CREATE TABLE dbo.Showing (
    ShowingId   INT IDENTITY(1,1) PRIMARY KEY,
    FilmId      INT NOT NULL REFERENCES dbo.Film(FilmId),
    ShowingDate DATE NOT NULL,
    ShowingTime TIME NOT NULL
);
GO

-- ─── Stored Procedures ────────────────────────────────────────────────────

-- Film -------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_GetAllFilms
AS
    SELECT FilmId, FilmTitle FROM dbo.Film ORDER BY FilmTitle;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetFilm
    @FilmId INT
AS
    SELECT FilmId, FilmTitle FROM dbo.Film WHERE FilmId = @FilmId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertFilm
    @FilmTitle NVARCHAR(50)
AS
    INSERT INTO dbo.Film (FilmTitle) VALUES (@FilmTitle);
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateFilm
    @FilmId    INT,
    @FilmTitle NVARCHAR(50)
AS
    UPDATE dbo.Film SET FilmTitle = @FilmTitle WHERE FilmId = @FilmId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteFilm
    @FilmId INT
AS
    DELETE FROM dbo.Film WHERE FilmId = @FilmId;
GO

-- Customer ---------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_GetAllCustomers
AS
    SELECT CustId, CustName FROM dbo.Customer ORDER BY CustName;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCustomer
    @CustId INT
AS
    SELECT CustId, CustName FROM dbo.Customer WHERE CustId = @CustId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertCustomer
    @CustName NVARCHAR(50)
AS
    INSERT INTO dbo.Customer (CustName) VALUES (@CustName);
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateCustomer
    @CustId   INT,
    @CustName NVARCHAR(50)
AS
    UPDATE dbo.Customer SET CustName = @CustName WHERE CustId = @CustId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteCustomer
    @CustId INT
AS
    DELETE FROM dbo.Customer WHERE CustId = @CustId;
GO

-- Showing ----------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_GetAllShowings
AS
    SELECT s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime,
           COALESCE(SUM(b.Seats), 0) AS SeatsBooked
    FROM dbo.Showing s
    INNER JOIN dbo.Film     f ON f.FilmId    = s.FilmId
    LEFT  JOIN dbo.Booking  b ON b.ShowingId = s.ShowingId
    GROUP BY s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime
    ORDER BY s.ShowingDate, s.ShowingTime;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetUpcomingShowings
AS
    SELECT s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime,
           COALESCE(SUM(b.Seats), 0) AS SeatsBooked
    FROM dbo.Showing s
    INNER JOIN dbo.Film    f ON f.FilmId    = s.FilmId
    LEFT  JOIN dbo.Booking b ON b.ShowingId = s.ShowingId
    WHERE s.ShowingDate >= CAST(GETDATE() AS DATE)
    GROUP BY s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime
    ORDER BY f.FilmTitle, s.ShowingDate, s.ShowingTime;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetShowing
    @ShowingId INT
AS
    SELECT s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime,
           COALESCE(SUM(b.Seats), 0) AS SeatsBooked
    FROM dbo.Showing s
    INNER JOIN dbo.Film    f ON f.FilmId    = s.FilmId
    LEFT  JOIN dbo.Booking b ON b.ShowingId = s.ShowingId
    WHERE s.ShowingId = @ShowingId
    GROUP BY s.ShowingId, s.FilmId, f.FilmTitle, s.ShowingDate, s.ShowingTime;
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertShowing
    @FilmId      INT,
    @ShowingDate DATE,
    @ShowingTime TIME
AS
    INSERT INTO dbo.Showing (FilmId, ShowingDate, ShowingTime)
    VALUES (@FilmId, @ShowingDate, @ShowingTime);
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateShowing
    @ShowingId   INT,
    @FilmId      INT,
    @ShowingDate DATE,
    @ShowingTime TIME
AS
    UPDATE dbo.Showing
    SET FilmId = @FilmId, ShowingDate = @ShowingDate, ShowingTime = @ShowingTime
    WHERE ShowingId = @ShowingId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteShowing
    @ShowingId INT
AS
    DELETE FROM dbo.Showing WHERE ShowingId = @ShowingId;
GO

-- Booking ----------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_GetAllBookings
AS
    SELECT b.BookingId, b.ShowingId, b.CustId, b.Seats,
           f.FilmTitle, c.CustName, s.ShowingDate, s.ShowingTime
    FROM dbo.Booking  b
    INNER JOIN dbo.Showing  s ON s.ShowingId = b.ShowingId
    INNER JOIN dbo.Film     f ON f.FilmId    = s.FilmId
    INNER JOIN dbo.Customer c ON c.CustId    = b.CustId
    ORDER BY s.ShowingDate, s.ShowingTime, c.CustName;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetBooking
    @BookingId INT
AS
    SELECT b.BookingId, b.ShowingId, b.CustId, b.Seats,
           f.FilmTitle, c.CustName, s.ShowingDate, s.ShowingTime
    FROM dbo.Booking  b
    INNER JOIN dbo.Showing  s ON s.ShowingId = b.ShowingId
    INNER JOIN dbo.Film     f ON f.FilmId    = s.FilmId
    INNER JOIN dbo.Customer c ON c.CustId    = b.CustId
    WHERE b.BookingId = @BookingId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertBooking
    @ShowingId INT,
    @CustId    INT,
    @Seats     SMALLINT,
    @Success   BIT OUTPUT,
    @Message   NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TotalSeats INT = 50;

    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @SeatsBooked INT;
        SELECT @SeatsBooked = COALESCE(SUM(Seats), 0)
        FROM dbo.Booking WITH (UPDLOCK, ROWLOCK)
        WHERE ShowingId = @ShowingId;

        IF (@Seats > @TotalSeats - @SeatsBooked)
        BEGIN
            SET @Success = 0;
            SET @Message = CONCAT('Sorry – only ', @TotalSeats - @SeatsBooked, ' seat(s) are available.');
            ROLLBACK TRANSACTION;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Booking (ShowingId, CustId, Seats)
            VALUES (@ShowingId, @CustId, @Seats);
            SET @Success = 1;
            SET @Message = 'Booking accepted.';
            COMMIT TRANSACTION;
        END
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @Success = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteBooking
    @BookingId INT
AS
    DELETE FROM dbo.Booking WHERE BookingId = @BookingId;
GO

-- ─── Seed Data ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Film)
BEGIN
    INSERT INTO dbo.Film (FilmTitle) VALUES
        ('The Dark Knight'), ('Inception'), ('Interstellar'), ('Tenet'), ('Oppenheimer');

    INSERT INTO dbo.Customer (CustName) VALUES
        ('Alice Smith'), ('Bob Jones'), ('Carol White'), ('David Brown'), ('Eve Taylor');

    INSERT INTO dbo.Showing (FilmId, ShowingDate, ShowingTime) VALUES
        (1, DATEADD(day, 1, CAST(GETDATE() AS DATE)), '14:00'),
        (1, DATEADD(day, 3, CAST(GETDATE() AS DATE)), '17:30'),
        (1, DATEADD(day, 7, CAST(GETDATE() AS DATE)), '20:00'),
        (2, DATEADD(day, 1, CAST(GETDATE() AS DATE)), '14:00'),
        (2, DATEADD(day, 3, CAST(GETDATE() AS DATE)), '17:30'),
        (2, DATEADD(day, 7, CAST(GETDATE() AS DATE)), '20:00'),
        (3, DATEADD(day, 1, CAST(GETDATE() AS DATE)), '14:00'),
        (3, DATEADD(day, 3, CAST(GETDATE() AS DATE)), '17:30'),
        (4, DATEADD(day, 2, CAST(GETDATE() AS DATE)), '20:00'),
        (5, DATEADD(day, 5, CAST(GETDATE() AS DATE)), '17:30');

    INSERT INTO dbo.Booking (ShowingId, CustId, Seats) VALUES
        (1, 1, 2), (1, 2, 3), (2, 3, 1), (4, 4, 4);
END
GO
