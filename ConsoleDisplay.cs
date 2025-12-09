namespace FolderContentAnalyzer;

public static class ConsoleDisplay
{
    public static void DisplayProgress(string currentPath)
    {
        var maxWidth = Console.WindowWidth - 1;
        var truncatedPath = TruncatePath(currentPath, maxWidth - 12);
        var progressText = $"Scanning: {truncatedPath}";
        var paddedText = progressText.PadRight(maxWidth);
        Console.Write($"\r{paddedText}");
    }

    public static void ClearProgress()
    {
        Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
    }

    public static void DisplayResults(List<FileSystemItem> items, long totalSize, int totalCount, int skippedCount)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;

        if (totalCount > items.Count)
        {
            Console.WriteLine($"Showing top {items.Count} of {totalCount} items (sorted by size)");
        }
        else
        {
            Console.WriteLine($"Analysis Complete - {totalCount} items found");
        }

        Console.WriteLine(new string('=', 50));
        Console.ResetColor();
        Console.WriteLine();

        foreach (var item in items)
        {
            var (value, unit) = SizeFormatter.Format(item.SizeInBytes);
            var color = GetColorForSize(unit);

            Console.ForegroundColor = color;
            var sizeString = SizeFormatter.FormatWithUnit(item.SizeInBytes);
            Console.Write($"{sizeString,-10}");
            Console.ResetColor();

            var typeIndicator = item.IsDirectory ? "[DIR] " : "      ";
            Console.WriteLine($"{typeIndicator}{item.FullPath}");
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Total Size: {SizeFormatter.FormatWithUnit(totalSize)}");

        if (skippedCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n{skippedCount} items skipped due to access errors");
        }

        Console.ResetColor();
    }

    public static void DisplayBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Folder Content Analyzer");
        Console.WriteLine("=======================");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static ConsoleColor GetColorForSize(SizeUnit unit)
    {
        return unit switch
        {
            SizeUnit.Gigabytes => ConsoleColor.Red,
            SizeUnit.Megabytes => ConsoleColor.Yellow,
            SizeUnit.Kilobytes => ConsoleColor.Green,
            SizeUnit.Bytes => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };
    }

    private static string TruncatePath(string path, int maxLength)
    {
        if (path.Length <= maxLength) return path;

        if (maxLength < 10) return path[..maxLength];

        var parts = path.Split(Path.DirectorySeparatorChar);
        if (parts.Length <= 2) return path[..maxLength];

        var filename = parts[^1];
        var drive = parts[0];

        if (filename.Length + drive.Length + 7 > maxLength)
        {
            return path[..maxLength];
        }

        return $"{drive}{Path.DirectorySeparatorChar}...{Path.DirectorySeparatorChar}{filename}";
    }
}
