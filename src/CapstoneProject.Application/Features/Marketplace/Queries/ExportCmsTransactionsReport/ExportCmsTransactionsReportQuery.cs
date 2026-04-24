using System.Globalization;
using System.Text;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsTransactionsReport;

public record ExportCmsTransactionsReportQuery(
    string Source = "marketplace",
    DateTime? From = null,
    DateTime? To = null,
    string Format = "csv"
) : IRequest<Result<CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto>>;

public class ExportCmsTransactionsReportQueryHandler : IRequestHandler<ExportCmsTransactionsReportQuery, Result<CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ExportCmsTransactionsReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto>> Handle(ExportCmsTransactionsReportQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(Domain.Enums.RoleEnum.Admin))
            return Result<CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto>.Failure("Chỉ quản trị viên mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        var from = NormalizeTimestamp(request.From ?? now.AddDays(-30));
        var to = NormalizeTimestamp(request.To ?? now);
        var source = request.Source.ToLowerInvariant();

        if (source == "orbitcoin")
        {
            var rows = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(pr => !pr.IsDeleted && pr.PaidAt != null && pr.PaidAt >= from && pr.PaidAt <= to)
                .OrderByDescending(pr => pr.PaidAt)
                .Select(pr => new ExportRow(pr.PaidAt!.Value, pr.Id.ToString(), pr.UserId.ToString(), "Wallet", pr.Amount, pr.AmountVnd ?? 0, pr.ExternalId ?? string.Empty))
                .ToListAsync(cancellationToken);
            return Result<CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto>.Success(BuildExport(rows, request.Format, "orbitcoin"), "Đã xuất giao dịch OrbitCoin.");
        }

        var paymentRows = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(pr => !pr.IsDeleted && pr.PaidAt != null && pr.PaidAt >= from && pr.PaidAt <= to)
            .OrderByDescending(pr => pr.PaidAt)
            .Select(pr => new ExportRow(pr.PaidAt!.Value, pr.Id.ToString(), pr.UserId.ToString(), pr.PaymentStatus.ToString(), pr.Amount, pr.AmountVnd ?? 0, pr.ExternalId ?? string.Empty))
            .ToListAsync(cancellationToken);

        return Result<CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto>.Success(BuildExport(paymentRows, request.Format, "marketplace"), "Đã xuất giao dịch marketplace.");
    }

    private sealed record ExportRow(DateTime CreatedAt, string Id, string UserId, string Type, decimal Amount, decimal ExtraValue, string Note);

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);

    private static CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto BuildExport(List<ExportRow> rows, string format, string source)
    {
        return format.ToLowerInvariant() switch
        {
            "xlsx" => BuildXlsx(rows, source),
            "pdf" => BuildPdf(rows, source),
            _ => BuildCsv(rows, source)
        };
    }

    private static CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto BuildCsv(List<ExportRow> rows, string source)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Report Name,Transactions Report");
        sb.AppendLine($"Source,{EscapeCsv(source)}");
        sb.AppendLine($"Generated At,{EscapeCsv(CapstoneProject.Domain.Common.VietnamDateTime.DbNow.ToString("yyyy-MM-dd HH:mm:ss"))}");
        sb.AppendLine();
        sb.AppendLine("Created At,Transaction ID,User ID,Type,Amount (OC),Amount (VND),Details");
        foreach (var row in rows)
            sb.AppendLine($"{row.CreatedAt:yyyy-MM-dd HH:mm:ss},{EscapeCsv(row.Id)},{EscapeCsv(row.UserId)},{EscapeCsv(row.Type)},{row.Amount.ToString(CultureInfo.InvariantCulture)},{row.ExtraValue.ToString(CultureInfo.InvariantCulture)},{EscapeCsv(row.Note)}");
        return new CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto
        {
            Content = Encoding.UTF8.GetBytes(sb.ToString()),
            ContentType = "text/csv",
            FileName = $"cms-{source}-transactions-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
        };
    }

    private static CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto BuildXlsx(List<ExportRow> rows, string source)
    {
        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(
                new SheetViews(new SheetView { WorkbookViewId = 0U }),
                new Columns(
                    new Column { Min = 1, Max = 1, Width = 20, CustomWidth = true },
                    new Column { Min = 2, Max = 3, Width = 40, CustomWidth = true },
                    new Column { Min = 4, Max = 4, Width = 20, CustomWidth = true },
                    new Column { Min = 5, Max = 6, Width = 16, CustomWidth = true },
                    new Column { Min = 7, Max = 7, Width = 40, CustomWidth = true }
                ),
                sheetData
            );
            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Transactions" });

            AppendRow(sheetData, "Transactions Report");
            AppendRow(sheetData, $"Source: {source}");
            AppendRow(sheetData, $"Generated At: {CapstoneProject.Domain.Common.VietnamDateTime.DbNow:yyyy-MM-dd HH:mm:ss}");
            AppendRow(sheetData, "");
            AppendRow(sheetData, "Created At", "Transaction ID", "User ID", "Type", "Amount (OC)", "Amount (VND)", "Details");
            foreach (var row in rows)
                AppendRow(sheetData, row.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), row.Id, row.UserId, row.Type, row.Amount.ToString("N2"), row.ExtraValue.ToString("N0"), row.Note);

            workbookPart.Workbook.Save();
        }

        return new CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto
        {
            Content = stream.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"cms-{source}-transactions-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        };
    }

    private static CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto BuildPdf(List<ExportRow> rows, string source)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.Header().Text($"Transactions Report ({source})").FontSize(18).Bold();
                page.Content().Column(col =>
                {
                    col.Item().Text($"Generated At: {CapstoneProject.Domain.Common.VietnamDateTime.DbNow:yyyy-MM-dd HH:mm:ss}");
                    col.Item().PaddingVertical(8);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("Created At");
                            header.Cell().Element(CellHeader).Text("Transaction ID");
                            header.Cell().Element(CellHeader).Text("User ID");
                            header.Cell().Element(CellHeader).Text("Type");
                            header.Cell().Element(CellHeader).AlignRight().Text("OC");
                            header.Cell().Element(CellHeader).AlignRight().Text("VND");
                            header.Cell().Element(CellHeader).Text("Details");
                        });
                        foreach (var row in rows.Take(400))
                        {
                            table.Cell().Element(CellBody).Text(row.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            table.Cell().Element(CellBody).Text(row.Id);
                            table.Cell().Element(CellBody).Text(row.UserId);
                            table.Cell().Element(CellBody).Text(row.Type);
                            table.Cell().Element(CellBody).AlignRight().Text($"{row.Amount:N2}");
                            table.Cell().Element(CellBody).AlignRight().Text($"{row.ExtraValue:N0}");
                            table.Cell().Element(CellBody).Text(row.Note);
                        }
                    });
                });
            });
        }).GeneratePdf();
        return new CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport.CmsExportFileDto
        {
            Content = bytes,
            ContentType = "application/pdf",
            FileName = $"cms-{source}-transactions-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
        };
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var escaped = value.Replace("\"", "\"\"");
        return escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n')
            ? $"\"{escaped}\""
            : escaped;
    }

    private static void AppendRow(SheetData sheetData, params string[] values)
    {
        var row = new Row();
        foreach (var value in values)
        {
            row.Append(new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(value ?? string.Empty))
            });
        }
        sheetData.Append(row);
    }

    private static IContainer CellHeader(IContainer container) =>
        container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(2);

    private static IContainer CellBody(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(2);
}
