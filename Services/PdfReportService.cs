using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BibliotekaInternetowa.Data;
using BibliotekaInternetowa.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaInternetowa.Services
{
    public class PdfReportService
    {
        private readonly ApplicationDbContext _context;

        public PdfReportService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateBorrowingsReport(string? status = null, string? userId = null)
        {
            var borrowings = _context.Borrowings
                .Include(b => b.User)
                .Include(b => b.Book)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "active":
                        borrowings = borrowings.Where(b => !b.IsReturned);
                        break;
                    case "returned":
                        borrowings = borrowings.Where(b => b.IsReturned);
                        break;
                    case "overdue":
                        borrowings = borrowings.Where(b => !b.IsReturned && b.DueDate < DateTime.Now);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                borrowings = borrowings.Where(b => b.UserId == userId);
            }

            var borrowingList = borrowings
                .OrderByDescending(b => b.BorrowDate)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Raport wypożyczeń")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Informacje o raporcie
                            column.Item().Text($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium);

                            if (!string.IsNullOrWhiteSpace(status))
                            {
                                column.Item().Text($"Status: {GetStatusName(status)}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Medium);
                            }

                            column.Item().Text($"Łączna liczba wypożyczeń: {borrowingList.Count}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10);

                            // Tabela wypożyczeń
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.5f);
                                });

                                // Nagłówek
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Użytkownik").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Książka").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Data wypożyczenia").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Termin zwrotu").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Data zwrotu").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                });

                                // Wiersze danych
                                foreach (var borrowing in borrowingList)
                                {
                                    var isOverdue = !borrowing.IsReturned && borrowing.DueDate < DateTime.Now;

                                    table.Cell().Element(CellStyle).Text(borrowing.User.UserName ?? "");
                                    table.Cell().Element(CellStyle).Text(borrowing.Book.Title);
                                    table.Cell().Element(CellStyle).Text(borrowing.BorrowDate.ToString("dd.MM.yyyy"));
                                    table.Cell().Element(CellStyle).Text(borrowing.DueDate.ToString("dd.MM.yyyy"));
                                    table.Cell().Element(CellStyle).Text(borrowing.ReturnDate?.ToString("dd.MM.yyyy") ?? "-");
                                    table.Cell().Element(CellStyle).Text(GetBorrowingStatus(borrowing, isOverdue));
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(TextStyle.Default.FontSize(9).FontColor(Colors.Grey.Medium))
                        .Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateStatisticsReport()
        {
            var totalBooks = _context.Books.Count();
            var totalUsers = _context.Users.Count();
            var totalBorrowings = _context.Borrowings.Count();
            var activeBorrowings = _context.Borrowings.Where(b => !b.IsReturned).Count();
            var overdueBorrowings = _context.Borrowings.Where(b => !b.IsReturned && b.DueDate < DateTime.Now).Count();

            var booksByCategory = _context.Books
                .GroupBy(b => b.Category)
                .Select(g => new { Category = g.Key ?? "Brak", Count = g.Count() })
                .ToList();

            var topBooks = _context.Borrowings
                .Include(b => b.Book)
                .GroupBy(b => new { b.BookId, b.Book.Title, b.Book.Author })
                .Select(g => new { g.Key.Title, g.Key.Author, Count = g.Count() })
                .OrderByDescending(b => b.Count)
                .Take(10)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Raport statystyk biblioteki")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            column.Item().Text($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium);

                            // Statystyki ogólne
                            column.Item().Text("Statystyki ogólne").SemiBold().FontSize(14);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Text("Wszystkie książki:");
                                table.Cell().Element(CellStyle).Text(totalBooks.ToString());

                                table.Cell().Element(CellStyle).Text("Użytkownicy:");
                                table.Cell().Element(CellStyle).Text(totalUsers.ToString());

                                table.Cell().Element(CellStyle).Text("Wszystkie wypożyczenia:");
                                table.Cell().Element(CellStyle).Text(totalBorrowings.ToString());

                                table.Cell().Element(CellStyle).Text("Aktywne wypożyczenia:");
                                table.Cell().Element(CellStyle).Text(activeBorrowings.ToString());

                                table.Cell().Element(CellStyle).Text("Przeterminowane:");
                                table.Cell().Element(CellStyle).Text(overdueBorrowings.ToString());
                            });

                            // Książki według kategorii
                            column.Item().PaddingTop(10).Text("Książki według kategorii").SemiBold().FontSize(14);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                foreach (var category in booksByCategory)
                                {
                                    table.Cell().Element(CellStyle).Text(category.Category);
                                    table.Cell().Element(CellStyle).Text(category.Count.ToString());
                                }
                            });

                            // Najpopularniejsze książki
                            column.Item().PaddingTop(10).Text("Top 10 najpopularniejszych książek").SemiBold().FontSize(14);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Tytuł").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Autor").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Liczba").SemiBold();
                                });

                                foreach (var book in topBooks)
                                {
                                    table.Cell().Element(CellStyle).Text(book.Title);
                                    table.Cell().Element(CellStyle).Text(book.Author);
                                    table.Cell().Element(CellStyle).Text(book.Count.ToString());
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(TextStyle.Default.FontSize(9).FontColor(Colors.Grey.Medium))
                        .Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(5);
        }

        private static string GetStatusName(string status)
        {
            return status switch
            {
                "active" => "Aktywne",
                "returned" => "Zwrócone",
                "overdue" => "Przeterminowane",
                _ => "Wszystkie"
            };
        }

        private static string GetBorrowingStatus(Borrowing borrowing, bool isOverdue)
        {
            if (borrowing.IsReturned)
                return "Zwrócona";
            if (isOverdue)
                return "Przeterminowana";
            return "Aktywna";
        }
    }
}

