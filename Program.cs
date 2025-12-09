namespace FolderContentAnalyzer;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            ConsoleDisplay.DisplayBanner();

            var directoryPath = InputHandler.GetDirectoryPath(args);

            Console.WriteLine($"Starting analysis of: {directoryPath}");
            Console.WriteLine();

            var progress = new Progress<string>(path =>
            {
                ConsoleDisplay.DisplayProgress(path);
            });

            var analyzer = new DirectoryAnalyzer();
            var results = analyzer.Analyze(directoryPath, progress);

            var totalSize = results.Sum(item => item.SizeInBytes);
            var displayItems = results.Take(40).ToList();

            ConsoleDisplay.DisplayResults(displayItems, totalSize, results.Count, analyzer.SkippedItemsCount);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nUnexpected error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
