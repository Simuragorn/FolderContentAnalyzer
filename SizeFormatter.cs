namespace FolderContentAnalyzer;

public static class SizeFormatter
{
    private const long KB = 1024;
    private const long MB = KB * 1024;
    private const long GB = MB * 1024;

    public static (double value, SizeUnit unit) Format(long bytes)
    {
        if (bytes >= GB)
            return ((double)bytes / GB, SizeUnit.Gigabytes);

        if (bytes >= MB)
            return ((double)bytes / MB, SizeUnit.Megabytes);

        if (bytes >= KB)
            return ((double)bytes / KB, SizeUnit.Kilobytes);

        return (bytes, SizeUnit.Bytes);
    }

    public static string FormatWithUnit(long bytes)
    {
        var (value, unit) = Format(bytes);

        var unitString = unit switch
        {
            SizeUnit.Gigabytes => "GB",
            SizeUnit.Megabytes => "MB",
            SizeUnit.Kilobytes => "KB",
            SizeUnit.Bytes => "B",
            _ => "B"
        };

        var formattedValue = unit == SizeUnit.Bytes
            ? value.ToString("F0")
            : value.ToString("F1");

        return $"{formattedValue} {unitString}";
    }
}
