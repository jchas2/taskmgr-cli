using System.Runtime.CompilerServices;

namespace Task.Manager.Internal.Abstractions.Tests;

public sealed class FileCleanupHelper : IDisposable
{
    private List<string> _tempDirs = new();
    private Lock _lock = new();

    ~FileCleanupHelper() => Dispose();

    public void Dispose()
    {
        foreach (string dir in _tempDirs) {
            try {
                try {
                    Directory.Delete(dir, recursive: true);
                }
                catch (UnauthorizedAccessException) {
                    DirectoryInfo dirInfo = new(dir);
                    foreach (FileSystemInfo fileInfo in dirInfo.EnumerateFileSystemInfos("*",
                                 SearchOption.AllDirectories)) {
                        fileInfo.Attributes = FileAttributes.Normal;
                    }

                    Directory.Delete(dir, recursive: true);
                }
                catch (DirectoryNotFoundException) {
                        
                }
            }
            catch { }
        }
    }

    private string GetTempDirectoryInternal([CallerMemberName] string memberName = "")
    {
        lock (_lock) {
            string tempDirectory = Path.Combine(Path.GetTempPath(), $"{memberName}-{Guid.NewGuid()}");
            _tempDirs.Add(tempDirectory);
            return tempDirectory;
        }
    } 
    
    public string GetTempDirectory([CallerMemberName] string memberName = "") => GetTempDirectoryInternal(memberName);

    public string GetTempFile(string extension)
    {
        if (!extension.StartsWith('.')) {
            extension = '.' + extension;
        }

        string tempFile = Path.Combine(
            GetTempDirectoryInternal(),
            Path.GetRandomFileName() + extension);
        
        return tempFile;
    }
}