using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BibliotekaInternetowa.Data;
using BibliotekaInternetowa.Models;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja logowania - ważne dla Azure
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddEventSourceLogger();
}

// KONTEKST + IDENTITY – TYLKO TEN KOD!
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    var errorMsg = "Connection string 'DefaultConnection' not found. Please configure it in Azure Portal -> Environment Variables -> Connection strings.";
    Console.WriteLine($"ERROR: {errorMsg}");
    throw new InvalidOperationException(errorMsg);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<BibliotekaInternetowa.Services.PdfReportService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Zastosuj migracje bazy danych
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Próba połączenia z bazą danych...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Test połączenia
        if (context.Database.CanConnect())
        {
            logger.LogInformation("Połączenie z bazą danych udane.");
        }
        else
        {
            logger.LogWarning("Nie można połączyć się z bazą danych.");
        }
        
        logger.LogInformation("Stosowanie migracji bazy danych...");
        context.Database.Migrate();
        logger.LogInformation("Migracje zostały zastosowane pomyślnie.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "BŁĄD podczas aplikowania migracji bazy danych: {Message}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        // W produkcji nie przerywamy działania aplikacji, ale logujemy błąd
        // W development rzucamy wyjątek, aby szybko zobaczyć problem
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// Seed ról i admina – DZIAŁA BEZ BŁĘDÓW
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        logger.LogInformation("Seedowanie ról i użytkownika admin...");
        
        string[] roleNames = { "Admin", "Czytelnik" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Utworzono rolę: {RoleName}", roleName);
            }
        }

        var adminEmail = "admin@biblioteka.pl";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Utworzono użytkownika admin: {Email}", adminEmail);
        }
        else
        {
            logger.LogInformation("Użytkownik admin już istnieje: {Email}", adminEmail);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "BŁĄD podczas seedowania ról i admina: {Message}", ex.Message);
        // Nie przerywamy działania aplikacji
    }
}

// Seed książek
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Seedowanie książek...");
        
        var booksToAdd = new List<Book>
    {
        new Book
        {
            Title = "Harry Potter i Kamień Filozoficzny",
            Author = "J.K. Rowling",
            ISBN = "978-83-7480-001-2",
            PublicationYear = 1997,
            Category = "Fantasy",
            Description = "Pierwsza część przygód młodego czarodzieja Harry'ego Pottera. Chłopiec odkrywa, że jest czarodziejem i rozpoczyna naukę w Szkole Magii i Czarodziejstwa w Hogwarcie.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788374800012-L.jpg",
            TotalCopies = 5,
            AvailableCopies = 5
        },
        new Book
        {
            Title = "Władca Pierścieni: Drużyna Pierścienia",
            Author = "J.R.R. Tolkien",
            ISBN = "978-83-7180-891-1",
            PublicationYear = 1954,
            Category = "Fantasy",
            Description = "Pierwsza część epickiej trylogii fantasy. Frodo Baggins otrzymuje zadanie zniszczenia Pierścienia Władzy, który może zniszczyć cały Śródziem.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788371808911-L.jpg",
            TotalCopies = 4,
            AvailableCopies = 3
        },
        new Book
        {
            Title = "1984",
            Author = "George Orwell",
            ISBN = "978-83-07-03302-0",
            PublicationYear = 1949,
            Category = "Science Fiction",
            Description = "Dystopijna powieść o totalitarnym społeczeństwie, gdzie każdy jest obserwowany przez Wielkiego Brata. Klasyka literatury antyutopijnej.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788307033020-L.jpg",
            TotalCopies = 6,
            AvailableCopies = 4
        },
        new Book
        {
            Title = "Zbrodnia i kara",
            Author = "Fiodor Dostojewski",
            ISBN = "978-83-08-06015-3",
            PublicationYear = 1866,
            Category = "Literatura piękna",
            Description = "Powieść psychologiczna o studencie Rodionie Raskolnikowie, który popełnia morderstwo i zmaga się z konsekwencjami swojego czynu.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788308060153-L.jpg",
            TotalCopies = 3,
            AvailableCopies = 2
        },
        new Book
        {
            Title = "Mały Książę",
            Author = "Antoine de Saint-Exupéry",
            ISBN = "978-83-08-05381-0",
            PublicationYear = 1943,
            Category = "Literatura dziecięca",
            Description = "Filozoficzna baśń o małym chłopcu, który podróżuje po planetach i uczy się o przyjaźni, miłości i odpowiedzialności.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788308053810-L.jpg",
            TotalCopies = 8,
            AvailableCopies = 6
        },
        new Book
        {
            Title = "Duma i uprzedzenie",
            Author = "Jane Austen",
            ISBN = "978-83-08-06020-7",
            PublicationYear = 1813,
            Category = "Romans",
            Description = "Klasyczna powieść obyczajowa o Elizabeth Bennet i panu Darcy. Historia miłości pełna nieporozumień i uprzedzeń.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788308060207-L.jpg",
            TotalCopies = 5,
            AvailableCopies = 5
        },
        new Book
        {
            Title = "Mistrz i Małgorzata",
            Author = "Michaił Bułhakow",
            ISBN = "978-83-08-06018-4",
            PublicationYear = 1967,
            Category = "Literatura piękna",
            Description = "Surrealistyczna powieść o wizycie diabła w Moskwie lat 30. XX wieku. Mieszanka realizmu, fantastyki i satyry.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788308060184-L.jpg",
            TotalCopies = 4,
            AvailableCopies = 3
        },
        new Book
        {
            Title = "Hobbit, czyli tam i z powrotem",
            Author = "J.R.R. Tolkien",
            ISBN = "978-83-7180-890-4",
            PublicationYear = 1937,
            Category = "Fantasy",
            Description = "Przygody hobbita Bilba Bagginsa, który wyrusza w niebezpieczną podróż, aby pomóc krasnoludom odzyskać ich skarb.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788371808904-L.jpg",
            TotalCopies = 6,
            AvailableCopies = 5
        },
        new Book
        {
            Title = "Sto lat samotności",
            Author = "Gabriel García Márquez",
            ISBN = "978-83-08-06019-1",
            PublicationYear = 1967,
            Category = "Literatura piękna",
            Description = "Epicka saga o rodzinie Buendía w fikcyjnym mieście Macondo. Arcydzieło realizmu magicznego.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788308060191-L.jpg",
            TotalCopies = 3,
            AvailableCopies = 2
        },
        new Book
        {
            Title = "Przeminęło z wiatrem",
            Author = "Margaret Mitchell",
            ISBN = "978-83-08-06021-4",
            PublicationYear = 1936,
            Category = "Romans",
            Description = "Epicka powieść o Scarlett O'Hara podczas wojny secesyjnej w Ameryce. Historia miłości, przetrwania i determinacji.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9788308060214-L.jpg",
            TotalCopies = 4,
            AvailableCopies = 1
        },
        new Book
        {
            Title = "Frankenstein",
            Author = "Mary Shelley",
            ISBN = "978-0-14-143947-1",
            PublicationYear = 1818,
            Category = "Horror",
            Description = "Klasyczna powieść gotycka o naukowcu Wiktorze Frankensteinie, który tworzy żywą istotę z części martwych ciał. Historia o odpowiedzialności naukowca, samotności i konsekwencjach ludzkiej ambicji.",
            CoverImageUrl = "https://covers.openlibrary.org/b/isbn/9780141439471-L.jpg",
            TotalCopies = 4,
            AvailableCopies = 3
        }
    };

        // Dodaj tylko te książki, których jeszcze nie ma w bazie (sprawdzenie po ISBN)
        foreach (var book in booksToAdd)
        {
            if (!await context.Books.AnyAsync(b => b.ISBN == book.ISBN))
            {
                context.Books.Add(book);
            }
        }
        
        await context.SaveChangesAsync();
        logger.LogInformation("Seedowanie książek zakończone pomyślnie.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "BŁĄD podczas seedowania książek: {Message}", ex.Message);
        // Nie przerywamy działania aplikacji
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();