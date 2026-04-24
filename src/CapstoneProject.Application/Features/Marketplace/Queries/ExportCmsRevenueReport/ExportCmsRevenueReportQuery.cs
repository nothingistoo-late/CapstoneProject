using System.Globalization;
using System.Text;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CapstoneProject.Application.Features.Marketplace.Queries.ExportCmsRevenueReport;

public record ExportCmsRevenueReportQuery(DateTime? From, DateTime? To, string GroupBy = "Day", string Format = "csv")
    : IRequest<Result<CmsExportFileDto>>;

public class CmsExportFileDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "export.bin";
}

public class ExportCmsRevenueReportQueryHandler : IRequestHandler<ExportCmsRevenueReportQuery, Result<CmsExportFileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ExportCmsRevenueReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CmsExportFileDto>> Handle(ExportCmsRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<CmsExportFileDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<CmsExportFileDto>.Failure("Chỉ quản trị viên mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        var from = NormalizeTimestamp(request.From ?? now.AddDays(-30));
        var to = NormalizeTimestamp(request.To ?? now);

        var rows = await _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .Where(pr => !pr.IsDeleted
                && pr.PaymentStatus == PaymentStatusEnum.Completed
                && pr.PaidAt != null
                && pr.PaidAt >= from
                && pr.PaidAt <= to)
            .Select(pr => new RevenueRowDto
            {
                PaidAt = pr.PaidAt!.Value,
                AmountVnd = pr.AmountVnd ?? 0
            })
            .ToListAsync(cancellationToken);

        var grouped = GroupRows(rows, request.GroupBy);
        var export = request.Format.ToLowerInvariant() switch
        {
            "xlsx" => BuildXlsx(grouped, from, to, request.GroupBy),
            "pdf" => BuildPdf(grouped, from, to, request.GroupBy),
            _ => BuildCsv(grouped, from, to, request.GroupBy)
        };

        return Result<CmsExportFileDto>.Success(export, "Đã xuất báo cáo doanh thu.");
    }

    private sealed class RevenueRowDto
    {
        public DateTime PaidAt { get; set; }
        public long AmountVnd { get; set; }
    }

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);

    private sealed class RevenueGroupedDto
    {
        public string Period { get; set; } = string.Empty;
        public long GrossVnd { get; set; }
        public long NetPlatformVnd { get; set; }
        public int Count { get; set; }
    }

    private static List<RevenueGroupedDto> GroupRows(List<RevenueRowDto> rows, string groupBy)
    {
        const decimal feeRate = 0.05m;
        return groupBy.ToLowerInvariant() switch
        {
            "year" => rows.GroupBy(x => x.PaidAt.Year)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueGroupedDto
                {
                    Period = g.Key.ToString(),
                    GrossVnd = g.Sum(x => x.AmountVnd),
                    NetPlatformVnd = (long)Math.Round(g.Sum(x => x.AmountVnd) * feeRate, MidpointRounding.AwayFromZero),
                    Count = g.Count()
                }).ToList(),
            "month" => rows.GroupBy(x => new { x.PaidAt.Year, x.PaidAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new RevenueGroupedDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    GrossVnd = g.Sum(x => x.AmountVnd),
                    NetPlatformVnd = (long)Math.Round(g.Sum(x => x.AmountVnd) * feeRate, MidpointRounding.AwayFromZero),
                    Count = g.Count()
                }).ToList(),
            "week" => rows.GroupBy(x =>
                {
                    var date = DateOnly.FromDateTime(x.PaidAt);
                    var dayOfWeek = (int)date.DayOfWeek;
                    var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                    return date.AddDays(-diff);
                })
                .OrderBy(g => g.Key)
                .Select(g => new RevenueGroupedDto
                {
                    Period = g.Key.ToString("yyyy-MM-dd"),
                    GrossVnd = g.Sum(x => x.AmountVnd),
                    NetPlatformVnd = (long)Math.Round(g.Sum(x => x.AmountVnd) * feeRate, MidpointRounding.AwayFromZero),
                    Count = g.Count()
                }).ToList(),
            _ => rows.GroupBy(x => DateOnly.FromDateTime(x.PaidAt))
                .OrderBy(g => g.Key)
                .Select(g => new RevenueGroupedDto
                {
                    Period = g.Key.ToString("yyyy-MM-dd"),
                    GrossVnd = g.Sum(x => x.AmountVnd),
                    NetPlatformVnd = (long)Math.Round(g.Sum(x => x.AmountVnd) * feeRate, MidpointRounding.AwayFromZero),
                    Count = g.Count()
                }).ToList()
        };
    }

    private static CmsExportFileDto BuildCsv(List<RevenueGroupedDto> rows, DateTime from, DateTime to, string groupBy)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Report Name,Platform Revenue Report");
        sb.AppendLine($"Generated At,{EscapeCsv(CapstoneProject.Domain.Common.VietnamDateTime.DbNow.ToString("yyyy-MM-dd HH:mm:ss"))}");
        sb.AppendLine($"Period From,{from:yyyy-MM-dd}");
        sb.AppendLine($"Period To,{to:yyyy-MM-dd}");
        sb.AppendLine($"Group By,{EscapeCsv(groupBy)}");
        sb.AppendLine();
        sb.AppendLine("Period,Gross Revenue (VND),Net Platform Revenue (VND),Transaction Count");
        foreach (var row in rows)
            sb.AppendLine($"{EscapeCsv(row.Period)},{row.GrossVnd},{row.NetPlatformVnd},{row.Count}");

        return new CmsExportFileDto
        {
            Content = Encoding.UTF8.GetBytes(sb.ToString()),
            ContentType = "text/csv",
            FileName = $"cms-revenue-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
        };
    }

    private static CmsExportFileDto BuildXlsx(List<RevenueGroupedDto> rows, DateTime from, DateTime to, string groupBy)
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
                new SheetFormatProperties { DefaultRowHeight = 16D },
                new Columns(
                    new Column { Min = 1, Max = 1, Width = 18, CustomWidth = true },
                    new Column { Min = 2, Max = 3, Width = 24, CustomWidth = true },
                    new Column { Min = 4, Max = 4, Width = 20, CustomWidth = true }
                ),
                sheetData
            );

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Revenue Report"
            });

            AppendRow(sheetData, "Platform Revenue Report");
            AppendRow(sheetData, $"Generated At: {CapstoneProject.Domain.Common.VietnamDateTime.DbNow:yyyy-MM-dd HH:mm:ss}");
            AppendRow(sheetData, $"Period: {from:yyyy-MM-dd} -> {to:yyyy-MM-dd}");
            AppendRow(sheetData, $"Group By: {groupBy}");
            AppendRow(sheetData, "");
            AppendRow(sheetData, "Period", "Gross Revenue (VND)", "Net Platform Revenue (VND)", "Transaction Count");
            foreach (var row in rows)
                AppendRow(sheetData, row.Period, row.GrossVnd.ToString("N0"), row.NetPlatformVnd.ToString("N0"), row.Count.ToString());

            workbookPart.Workbook.Save();
        }

        return new CmsExportFileDto
        {
            Content = stream.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"cms-revenue-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        };
    }

    private static CmsExportFileDto BuildPdf(List<RevenueGroupedDto> rows, DateTime from, DateTime to, string groupBy)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var totalGross = rows.Sum(r => r.GrossVnd);
        var totalNet = rows.Sum(r => r.NetPlatformVnd);
        var totalCount = rows.Sum(r => r.Count);
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.Header().Text("Platform Revenue Report").FontSize(18).Bold();
                page.Content().Column(col =>
                {
                    col.Item().Text($"Generated At: {CapstoneProject.Domain.Common.VietnamDateTime.DbNow:yyyy-MM-dd HH:mm:ss}");
                    col.Item().Text($"Period: {from:yyyy-MM-dd} -> {to:yyyy-MM-dd} | Group By: {groupBy}");
                    col.Item().PaddingVertical(6);
                    col.Item().Text($"Total Gross: {totalGross:N0} VND | Total Net: {totalNet:N0} VND | Total Transactions: {totalCount}");
                    col.Item().PaddingVertical(8);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("Period");
                            header.Cell().Element(CellHeader).AlignRight().Text("Gross Revenue (VND)");
                            header.Cell().Element(CellHeader).AlignRight().Text("Net Platform Revenue (VND)");
                            header.Cell().Element(CellHeader).AlignRight().Text("Transactions");
                        });
                        foreach (var row in rows)
                        {
                            table.Cell().Element(CellBody).Text(row.Period);
                            table.Cell().Element(CellBody).AlignRight().Text($"{row.GrossVnd:N0}");
                            table.Cell().Element(CellBody).AlignRight().Text($"{row.NetPlatformVnd:N0}");
                            table.Cell().Element(CellBody).AlignRight().Text(row.Count.ToString());
                        }
                    });
                });
            });
        }).GeneratePdf();
        return new CmsExportFileDto
        {
            Content = bytes,
            ContentType = "application/pdf",
            FileName = $"cms-revenue-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
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
        container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten1).PaddingVertical(4).PaddingHorizontal(2);

    private static IContainer CellBody(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(2);
}
