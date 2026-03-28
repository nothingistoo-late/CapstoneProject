using System.Text;
using System.Text.RegularExpressions;

namespace CapstoneProject.API.Extensions;

/// <summary>
/// Chuyển một câu INSERT kiểu SQL Server (script SSMS: [dbo].[Table], N'...', CAST(... AS DateTime), bit 0/1)
/// sang PostgreSQL cho bảng seed Maps / MapDetails / Hints / MapTags.
/// MapTags dùng ON CONFLICT (MapId, TagId) để idempotent với dữ liệu đã có (seed cũ / app).
/// </summary>
internal static class SqlServerToPostgreSqlInsertConverter
{
    private static readonly Dictionary<string, HashSet<string>> BoolColumnsByTable =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Maps"] = new(StringComparer.OrdinalIgnoreCase) { "IsPublished", "IsDeleted" },
            ["MapDetails"] = new(StringComparer.OrdinalIgnoreCase) { "IsDeleted" },
            ["MapTags"] = new(StringComparer.OrdinalIgnoreCase) { "IsDeleted" },
            ["Hints"] = new(StringComparer.OrdinalIgnoreCase) { "IsDeleted" },
        };

    private static readonly Regex InsertHeaderRegex = new(
        @"INSERT\s+\[dbo\]\.\[(\w+)\]\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ValuesClauseRegex = new(
        @"\)\s+VALUES\s*\(",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex ColumnBracketRegex = new(
        @"\[([^\]]+)\]",
        RegexOptions.Compiled);

    private static readonly Regex GuidLiteralRegex = new(
        @"^[0-9a-fA-F-]{36}$",
        RegexOptions.Compiled);

    public static string ConvertInsertStatement(string statement)
    {
        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException("Empty statement.", nameof(statement));

        var s = statement.Trim();

        s = Regex.Replace(
            s,
            @"CAST\s*\(\s*N'((?:[^']|'')*)'\s+AS\s+DateTime2(?:\([^)]*\))?\s*\)",
            m => "'" + UnescapeSqlString(m.Groups[1].Value) + "'::timestamptz",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        s = Regex.Replace(
            s,
            @"CAST\s*\(\s*N'((?:[^']|'')*)'\s+AS\s+DateTime\s*\)",
            m => "'" + UnescapeSqlString(m.Groups[1].Value) + "'::timestamptz",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        s = Regex.Replace(
            s,
            @"CAST\s*\(\s*([\d.]+)\s+AS\s+Decimal\s*\([^)]+\)\s*\)",
            "$1",
            RegexOptions.IgnoreCase);

        var header = InsertHeaderRegex.Match(s);
        if (!header.Success)
            throw new InvalidOperationException("Expected INSERT [dbo].[TableName] (...).");

        var table = header.Groups[1].Value;
        if (!BoolColumnsByTable.ContainsKey(table))
            throw new InvalidOperationException($"Unsupported table for conversion: {table}.");

        var valuesMatch = ValuesClauseRegex.Match(s);
        if (!valuesMatch.Success)
            throw new InvalidOperationException("Expected ) VALUES ( after column list.");

        int colListStart = header.Index + header.Length;
        int colListEnd = valuesMatch.Index;
        var columnListSegment = s.Substring(colListStart, colListEnd - colListStart);
        var columns = ColumnBracketRegex.Matches(columnListSegment)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();

        int valuesOpenParen = valuesMatch.Index + valuesMatch.Length - 1;
        var valueTokens = SplitValuesTokens(s, valuesOpenParen);
        if (columns.Count != valueTokens.Count)
            throw new InvalidOperationException(
                $"Column count ({columns.Count}) does not match value count ({valueTokens.Count}) for table {table}.");

        var boolCols = BoolColumnsByTable[table];
        var converted = new string[valueTokens.Count];
        for (int i = 0; i < valueTokens.Count; i++)
            converted[i] = ConvertValueToken(valueTokens[i].Trim(), columns[i], boolCols);

        var sb = new StringBuilder();
        sb.Append("INSERT INTO \"").Append(table).Append("\" (");
        for (int i = 0; i < columns.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('"').Append(columns[i]).Append('"');
        }

        sb.Append(") VALUES (");
        for (int i = 0; i < converted.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(converted[i]);
        }

        // MapTags: unique IX_MapTags_MapId_TagId — re-seed / app may already have (MapId, TagId) with another Id.
        if (string.Equals(table, "MapTags", StringComparison.OrdinalIgnoreCase))
            sb.Append(") ON CONFLICT (\"MapId\", \"TagId\") DO NOTHING");
        else
            sb.Append(") ON CONFLICT (\"Id\") DO NOTHING");
        return sb.ToString();
    }

    private static string UnescapeSqlString(string inner)
    {
        return inner.Replace("''", "'", StringComparison.Ordinal);
    }

    private static string ConvertValueToken(string token, string columnName, HashSet<string> boolCols)
    {
        if (string.Equals(token, "NULL", StringComparison.OrdinalIgnoreCase))
            return "NULL";

        if (boolCols.Contains(columnName) && (token == "0" || token == "1"))
            return token == "0" ? "false" : "true";

        if (token.Length >= 3 &&
            token.StartsWith("N'", StringComparison.OrdinalIgnoreCase) &&
            token[^1] == '\'')
        {
            var inner = token.Substring(2, token.Length - 3);
            inner = UnescapeSqlString(inner);
            if (GuidLiteralRegex.IsMatch(inner))
                return "'" + inner + "'::uuid";
            var escaped = inner.Replace("'", "''", StringComparison.Ordinal);
            return "'" + escaped + "'";
        }

        return token;
    }

    /// <summary>
    /// Tách đối số của VALUES (...), tôn trọng chuỗi N'...' (có thể nhiều dòng) và dấu '' trong chuỗi.
    /// </summary>
    private static List<string> SplitValuesTokens(string fullStatement, int valuesOpenParenIndex)
    {
        int i = valuesOpenParenIndex + 1;
        var tokens = new List<string>();
        var sb = new StringBuilder();
        int depth = 1;

        while (i < fullStatement.Length && depth > 0)
        {
            char c = fullStatement[i];

            if (depth == 1 && c == 'N' && i + 1 < fullStatement.Length && fullStatement[i + 1] == '\'')
            {
                if (sb.Length > 0)
                {
                    var t = sb.ToString().Trim();
                    if (t.Length > 0)
                        tokens.Add(t);
                    sb.Clear();
                }

                var strStart = i;
                i += 2;
                while (i < fullStatement.Length)
                {
                    if (fullStatement[i] == '\'' &&
                        (i + 1 >= fullStatement.Length || fullStatement[i + 1] != '\''))
                    {
                        i++;
                        break;
                    }

                    if (i + 1 < fullStatement.Length &&
                        fullStatement[i] == '\'' &&
                        fullStatement[i + 1] == '\'')
                    {
                        i += 2;
                        continue;
                    }

                    i++;
                }

                tokens.Add(fullStatement.Substring(strStart, i - strStart));
                continue;
            }

            if (c == '(')
            {
                depth++;
                sb.Append(c);
                i++;
                continue;
            }

            if (c == ')')
            {
                depth--;
                if (depth == 0)
                {
                    i++;
                    break;
                }

                sb.Append(c);
                i++;
                continue;
            }

            if (c == ',' && depth == 1)
            {
                // Sau khi đọc xong N'...', sb vẫn rỗng; phẩy chỉ là phân cách — không thêm token rỗng.
                var t = sb.ToString().Trim();
                if (t.Length > 0)
                    tokens.Add(t);
                sb.Clear();
                i++;
                continue;
            }

            sb.Append(c);
            i++;
        }

        if (sb.Length > 0)
        {
            var t = sb.ToString().Trim();
            if (t.Length > 0)
                tokens.Add(t);
        }

        return tokens;
    }
}
