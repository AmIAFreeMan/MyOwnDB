
using ferretDB.Core.Common;

namespace ferretDB.Core.Storage;

/// <summary>
/// Manages the physical reading, writing, and memory caching of 4KB disk pages.
/// </summary>
public class Pager : IDisposable
{
    // A standard 4KB page size balances memory usage and disk I/O efficiency.
    public const int PageSize = 4096;

    // For this MVP, we cap the database at 100 pages (approx 400KB of data per table)
    public const int MaxPages = 100;

    private readonly FileStream _fileStream;
    private readonly long _fileLength;

    // The Page Cache: An array of byte arrays representing our in-memory pages
    private readonly byte[][] _pages;

    public Pager(string filename)
    {
        // Open the file, or create it if it doesn't exist. Keep it open for Read/Write.
        _fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _fileLength = _fileStream.Length;
        _pages = new byte[MaxPages][];
    }

    /// <summary>
    /// Retrieves a requested page. If it is not in memory, it loads it from disk.
    /// </summary>
    public byte[] GetPage(uint pageNumber)
    {
        if (pageNumber >= MaxPages)
        {
            throw new FerretException($"Page number {pageNumber} out of bounds.", FerretErrorCode.StorageError);
        }

        // Cache Miss: The page is not in memory yet.
        if (_pages[pageNumber] == null)
        {
            byte[] page = new byte[PageSize];

            // Calculate how many full pages currently exist in the file
            uint numPages = (uint)(_fileLength / PageSize);

            // We might have a partial page at the end of the file
            if (_fileLength % PageSize != 0)
            {
                numPages++;
            }

            // If the requested page exists in the physical file, read it.
            // If it doesn't, we just return the blank byte array (a new, empty page).
            if (pageNumber <= numPages)
            {
                _fileStream.Seek(pageNumber * PageSize, SeekOrigin.Begin);
                _fileStream.Read(page, 0, PageSize);
            }

            // Save to cache
            _pages[pageNumber] = page;
        }

        // Cache Hit: Return the page from memory
        return _pages[pageNumber];
    }

    /// <summary>
    /// Writes a specific page from memory back to the physical disk.
    /// </summary>
    public void Flush(uint pageNumber)
    {
        if (_pages[pageNumber] == null) return;

        _fileStream.Seek(pageNumber * PageSize, SeekOrigin.Begin);
        _fileStream.Write(_pages[pageNumber], 0, PageSize);
        _fileStream.Flush();
    }

    /// <summary>
    /// Cleans up resources when the database shuts down, ensuring all modified memory is saved.
    /// </summary>
    public void Dispose()
    {
        for (uint i = 0; i < MaxPages; i++)
        {
            if (_pages[i] != null)
            {
                Flush(i);
            }
        }

        _fileStream.Close();
    }
}