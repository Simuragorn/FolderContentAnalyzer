using System.Collections.Concurrent;

namespace FolderContentAnalyzer;

public class DirectoryAnalyzer
{
    private DateTime _lastProgressUpdate = DateTime.MinValue;
    private const int ProgressThrottleMs = 50;
    private int _skippedItemsCount;
    private readonly object _progressLock = new object();

    public int SkippedItemsCount => _skippedItemsCount;

    public List<FileSystemItem> Analyze(string rootPath, IProgress<string>? progress)
    {
        var results = new ConcurrentBag<FileSystemItem>();
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

            var directories = Directory.EnumerateDirectories(rootPath).ToList();

            Parallel.ForEach(directories, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },
            directoryPath =>
            {
                ReportProgress(progress, directoryPath);
                var directorySize = CalculateDirectorySize(directoryPath, progress);
                var directoryName = Path.GetFileName(directoryPath);
                results.Add(new FileSystemItem(directoryPath, directoryName, directorySize, true));
            });
        }
        catch (UnauthorizedAccessException)
        {
            Interlocked.Increment(ref _skippedItemsCount);
        }
        catch (PathTooLongException)
        {
            Interlocked.Increment(ref _skippedItemsCount);
        }
        catch (IOException)
        {
            Interlocked.Increment(ref _skippedItemsCount);
        }

        var sortedResults = results.ToList();
        sortedResults.Sort();

        return sortedResults;
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
            Interlocked.Increment(ref _skippedItemsCount);
        }
        catch (PathTooLongException)
        {
            Interlocked.Increment(ref _skippedItemsCount);
        }
        catch (IOException)
        {
            Interlocked.Increment(ref _skippedItemsCount);
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
            Interlocked.Increment(ref _skippedItemsCount);
            return false;
        }
        catch (PathTooLongException)
        {
            Interlocked.Increment(ref _skippedItemsCount);
            return false;
        }
        catch (IOException)
        {
            Interlocked.Increment(ref _skippedItemsCount);
            return false;
        }
    }

    private void ReportProgress(IProgress<string>? progress, string path)
    {
        if (progress == null) return;

        lock (_progressLock)
        {
            var now = DateTime.UtcNow;
            if ((now - _lastProgressUpdate).TotalMilliseconds >= ProgressThrottleMs)
            {
                progress.Report(path);
                _lastProgressUpdate = now;
            }
        }
    }
}
