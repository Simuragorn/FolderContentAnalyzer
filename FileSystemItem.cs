namespace FolderContentAnalyzer;

public record FileSystemItem(string FullPath, string Name, long SizeInBytes, bool IsDirectory)
    : IComparable<FileSystemItem>
{
    public int CompareTo(FileSystemItem? other)
    {
        if (other == null) return 1;
        return other.SizeInBytes.CompareTo(SizeInBytes);
    }
}
