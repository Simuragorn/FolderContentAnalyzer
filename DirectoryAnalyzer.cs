namespace FolderContentAnalyzer;

public class DirectoryAnalyzer
{
    private DateTime _lastProgressUpdate = DateTime.MinValue;
    private const int ProgressThrottleMs = 50;
    private int _skippedItemsCount;

    public int SkippedItemsCount => _skippedItemsCount;

    public List<FileSystemItem> Analyze(string rootPath, IProgress<string>? progress)
    {
        var results = new List<FileSystemItem>();
        _skippedItemsCount = 0;

        try
        {
            var files = Directory.EnumerateFiles(rootPath);
            foreach (var filePath in files)
            {
                if (TryGetFileSize(filePath, out var fileSize))
                {
                    ReportProgress(progress, filePath);
                    var fileName = Path.GetFileName(filePath);
                    results.Add(new FileSystemItem(filePath, fileName, fileSize, false));
                }
            }

            var directories = Directory.EnumerateDirectories(rootPath);
            foreach (var directoryPath in directories)
            {
                ReportProgress(progress, directoryPath);
                var directorySize = CalculateDirectorySize(directoryPath, progress);
                var directoryName = Path.GetFileName(directoryPath);
                results.Add(new FileSystemItem(directoryPath, directoryName, directorySize, true));
            }
        }
        catch (UnauthorizedAccessException)
        {
            _skippedItemsCount++;
        }
        catch (PathTooLongException)
        {
            _skippedItemsCount++;
        }
        catch (IOException)
        {
            _skippedItemsCount++;
        }

        results.Sort();

        return results;
    }

    private long CalculateDirectorySize(string directoryPath, IProgress<string>? progress)
    {
        long totalSize = 0;

        try
        {
            var files = Directory.EnumerateFiles(directoryPath);
            foreach (var filePath in files)
            {
                if (TryGetFileSize(filePath, out var fileSize))
                {
                    ReportProgress(progress, filePath);
                    totalSize += fileSize;
                }
            }

            var subdirectories = Directory.EnumerateDirectories(directoryPath);
            foreach (var subdirectoryPath in subdirectories)
            {
                ReportProgress(progress, subdirectoryPath);
                totalSize += CalculateDirectorySize(subdirectoryPath, progress);
            }
        }
        catch (UnauthorizedAccessException)
        {
            _skippedItemsCount++;
        }
        catch (PathTooLongException)
        {
            _skippedItemsCount++;
        }
        catch (IOException)
        {
            _skippedItemsCount++;
        }

        return totalSize;
    }

    private bool TryGetFileSize(string filePath, out long size)
    {
        size = 0;
        try
        {
            var fileInfo = new FileInfo(filePath);
            size = fileInfo.Length;
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            _skippedItemsCount++;
            return false;
        }
        catch (PathTooLongException)
        {
            _skippedItemsCount++;
            return false;
        }
        catch (IOException)
        {
            _skippedItemsCount++;
            return false;
        }
    }

    private void ReportProgress(IProgress<string>? progress, string path)
    {
        if (progress == null) return;

        var now = DateTime.UtcNow;
        if ((now - _lastProgressUpdate).TotalMilliseconds >= ProgressThrottleMs)
        {
            progress.Report(path);
            _lastProgressUpdate = now;
        }
    }
}
