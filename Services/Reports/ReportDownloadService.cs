using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillForge.Interfaces;
using SkillForge.Models;
using System.Threading.Tasks;

namespace SkillForge.Services.Reports
{
    public class ReportDownloadService : IReportDownloadService
    {
        public async Task<byte[]> GenerateCourseFinancialReportPdfAsync(CourseFinancialReportVM data)
        {
            return await Task.Run(() =>
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        RenderHeader(page, $"{data.CourseTitle} Financial Report", data.Instructor);
                        
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // Course Summary Section
                            col.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Course Details").Bold().FontSize(12).FontColor(Colors.Purple.Medium);
                                    c.Item().Text($"Category: {data.Category}");
                                    c.Item().Text($"Level: {data.Level}");
                                    c.Item().Text($"Status: {data.Status}");
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Publish Date: {data.PublishDate}");
                                    c.Item().Text($"Base Price: ₹{data.BasePrice:N0}");
                                    c.Item().Text($"Discount: {data.DiscountPercent}%");
                                    c.Item().Text($"Selling Price: ₹{data.SellingPrice:N0}");
                                });
                            });

                            // Financial Summary Boxes
                            col.Item().PaddingVertical(10).Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Students").FontSize(9); c.Item().Text(data.TotalStudents.ToString()).Bold().FontSize(14); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Gross Revenue").FontSize(9); c.Item().Text($"₹{data.GrossRevenue:N0}").Bold().FontSize(14); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Platform Fee (20%)").FontSize(9); c.Item().Text($"₹{data.PlatformFee:N0}").Bold().FontSize(14).FontColor(Colors.Red.Medium); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Net Earnings").FontSize(9); c.Item().Text($"₹{data.NetEarnings:N0}").Bold().FontSize(14).FontColor(Colors.Green.Medium); });
                            });

                            col.Item().PaddingTop(10).Text("Transaction History").Bold().FontSize(12).FontColor(Colors.Purple.Medium);

                            // Main Table
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#");
                                    header.Cell().Element(CellStyle).Text("Student");
                                    header.Cell().Element(CellStyle).Text("Email");
                                    header.Cell().Element(CellStyle).Text("Date");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Paid");

                                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.Bold().FontSize(10)).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                });

                                int i = 1;
                                foreach (var t in data.Transactions)
                                {
                                    table.Cell().Element(RowStyle).Text(i++.ToString());
                                    table.Cell().Element(RowStyle).Text(t.StudentName);
                                    table.Cell().Element(RowStyle).Text(t.StudentEmail);
                                    table.Cell().Element(RowStyle).Text(t.EnrollmentDate);
                                    table.Cell().Element(RowStyle).AlignRight().Text($"₹{t.AmountPaid:N0}");

                                    static IContainer RowStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).DefaultTextStyle(x => x.FontSize(10));
                                }
                            });
                        });

                        RenderFooter(page);
                    });
                });
                return doc.GeneratePdf();
            });
        }

        public async Task<byte[]> GenerateStudentListPdfAsync(CourseStudentListReportVM data)
        {
            return await Task.Run(() =>
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        RenderHeader(page, $"Enrolled Students for {data.CourseTitle} Course", data.Instructor);

                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            col.Item().PaddingBottom(10).Text(t =>
                            {
                                t.Span("Category: ").Bold(); t.Span(data.Category);
                                t.Span("  |  ").Bold();
                                t.Span("Total Active Students: ").Bold(); t.Span(data.TotalStudents.ToString());
                            });

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#");
                                    header.Cell().Element(CellStyle).Text("Student Name");
                                    header.Cell().Element(CellStyle).Text("Email");
                                    header.Cell().Element(CellStyle).Text("Mobile");
                                    header.Cell().Element(CellStyle).Text("Enrolled Date");

                                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.Bold().FontSize(10)).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                });

                                int i = 1;
                                foreach (var s in data.Students)
                                {
                                    table.Cell().Element(RowStyle).Text(i++.ToString());
                                    table.Cell().Element(RowStyle).Text(s.Name);
                                    table.Cell().Element(RowStyle).Text(s.Email);
                                    table.Cell().Element(RowStyle).Text(s.Mobile ?? "-");
                                    table.Cell().Element(RowStyle).Text(s.EnrollmentDate);

                                    static IContainer RowStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).DefaultTextStyle(x => x.FontSize(10));
                                }
                            });
                        });
                        RenderFooter(page);
                    });
                });
                return doc.GeneratePdf();
            });
        }

        public async Task<byte[]> GenerateMonthlyFinancialReportPdfAsync(MonthlyFinancialReportVM data)
        {
            return await Task.Run(() =>
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        RenderHeader(page, $"{data.ReportMonth} Financial Report", data.Instructor);

                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // Monthly Summary Boxes
                            col.Item().PaddingVertical(10).Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Total Courses").FontSize(9); c.Item().Text(data.TotalCourses.ToString()).Bold().FontSize(14); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Gross Sales").FontSize(9); c.Item().Text($"₹{data.TotalEarning:N0}").Bold().FontSize(14); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Platform Fee").FontSize(9); c.Item().Text($"₹{data.PlatformFee:N0}").Bold().FontSize(14).FontColor(Colors.Red.Medium); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Net Share").FontSize(9); c.Item().Text($"₹{data.NetEarnings:N0}").Bold().FontSize(14).FontColor(Colors.Green.Medium); });
                            });

                            col.Item().PaddingTop(10).Text("Course-wise Monthly Breakdown").Bold().FontSize(12).FontColor(Colors.Purple.Medium);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Course Name");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Students");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Gross");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Fee");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Net");

                                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.Bold().FontSize(10)).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                });

                                foreach (var b in data.CourseBreakdowns)
                                {
                                    table.Cell().Element(RowStyle).Text(b.CourseName);
                                    table.Cell().Element(RowStyle).AlignRight().Text(b.NewStudents.ToString());
                                    table.Cell().Element(RowStyle).AlignRight().Text($"₹{b.GrossRevenue:N0}");
                                    table.Cell().Element(RowStyle).AlignRight().Text($"₹{b.PlatformFee:N0}");
                                    table.Cell().Element(RowStyle).AlignRight().Text($"₹{b.NetEarnings:N0}");

                                    static IContainer RowStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).DefaultTextStyle(x => x.FontSize(10));
                                }
                            });
                        });
                        RenderFooter(page);
                    });
                });
                return doc.GeneratePdf();
            });
        }

        public async Task<byte[]> GenerateInstructorGlobalCourseReportPdfAsync(InstructorGlobalCourseReportVM data)
        {
            return await Task.Run(() =>
            {
                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        RenderHeader(page, "Global Course Performance Report", data.Instructor);

                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // Global Summary Boxes
                            col.Item().PaddingVertical(10).Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Total Courses").FontSize(9); c.Item().Text(data.CourseEarnings.Count.ToString()).Bold().FontSize(14); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Lifetime Gross").FontSize(9); c.Item().Text($"₹{data.TotalGrossRevenue:N0}").Bold().FontSize(14); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Platform cut").FontSize(9); c.Item().Text($"₹{data.TotalPlatformFee:N0}").Bold().FontSize(14).FontColor(Colors.Red.Medium); });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(c => { c.Item().Text("Lifetime Net").FontSize(9); c.Item().Text($"₹{data.TotalNetEarnings:N0}").Bold().FontSize(14).FontColor(Colors.Green.Medium); });
                            });

                            col.Item().PaddingTop(10).Text("Course-wise Lifetime Performance").Bold().FontSize(12).FontColor(Colors.Purple.Medium);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Course Title");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Students");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Avg Price");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Gross");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Net Share");

                                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.Bold().FontSize(10)).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                });

                                foreach (var b in data.CourseEarnings)
                                {
                                    table.Cell().Element(RowStyle).Text(b.CourseTitle);
                                    table.Cell().Element(RowStyle).AlignRight().Text(b.EnrolledStudents.ToString());
                                    table.Cell().Element(RowStyle).AlignRight().Text($"₹{b.PricePerStudent:N0}");
                                    table.Cell().Element(RowStyle).AlignRight().Text($"₹{b.GrossRevenue:N0}");
                                    table.Cell().Element(RowStyle).AlignRight().Text($"₹{b.InstructorEarnings:N0}");

                                    static IContainer RowStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).DefaultTextStyle(x => x.FontSize(10));
                                }
                            });
                        });
                        RenderFooter(page);
                    });
                });
                return doc.GeneratePdf();
            });
        }

        private void RenderHeader(PageDescriptor page, string title, InstructorInfoVM instructor)
        {
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("SkillForge").FontSize(24).Bold().FontColor(Colors.Purple.Medium);
                    col.Item().Text(title).FontSize(14).SemiBold();
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Date: {System.DateTime.Now:dd MMM yyyy}").FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().Text(instructor.Name).SemiBold().FontSize(11);
                    col.Item().Text(instructor.Email).FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(instructor.Mobile)) col.Item().Text(instructor.Mobile).FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private void RenderFooter(PageDescriptor page)
        {
            page.Footer().PaddingTop(10).AlignCenter().Text(x =>
            {
                x.Span("SkillForge Platform - Private & Confidential | Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                x.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                x.Span(" of ").FontSize(9).FontColor(Colors.Grey.Medium);
                x.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }
    }
}
