namespace FolderContentAnalyzer;

public static class InputHandler
{
    public static string GetDirectoryPath(string[] args)
    {
        string? path = null;

        if (args.Length > 0)
        {
            path = args[0];
            if (ValidateDirectory(path, out var validPath))
            {
                return validPath;
            }
        }

        while (true)
        {
            Console.Write("Enter directory path: ");
            path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Path cannot be empty");
                Console.ResetColor();
                continue;
            }

            if (ValidateDirectory(path, out var validPath))
            {
                return validPath;
            }
        }
    }

    private static bool ValidateDirectory(string path, out string validPath)
    {
        validPath = string.Empty;

        try
        {
            var fullPath = Path.GetFullPath(path);

            if (!Directory.Exists(fullPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Directory not found: {fullPath}");
                Console.ResetColor();
                return false;
            }

            if (File.Exists(fullPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Path is a file, not a directory: {fullPath}");
                Console.ResetColor();
                return false;
            }

            validPath = fullPath;
            return true;
        }
        catch (ArgumentException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Invalid path format: {path}");
            Console.ResetColor();
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Access denied to path: {path}");
            Console.ResetColor();
            return false;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }
}
