using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace TimescaleWebAPI.Application.Validators;

public class CustomDateTimeConverter : DateTimeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new CsvHelperException(row.Context, "Date cannot be empty");

        var formats = new[]
        {
            "yyyy-MM-ddTHH-mm-ss.ffffZ",
            "yyyy-MM-ddTHH-mm-ss.fffZ",
            "yyyy-MM-ddTHH-mm-ss.ffZ",
            "yyyy-MM-ddTHH-mm-ss.fZ",
            "yyyy-MM-ddTHH-mm-ssZ",
            "yyyy-MM-ddTHH-mm-ss"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var result))
            {
                return result;
            }
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var dt))
        {
            return dt;
        }

        throw new CsvHelperException(row.Context,
            $"Cannot convert '{text}' to DateTime. Expected format: yyyy-MM-ddTHH-mm-ss.ffffZ");
    }
}