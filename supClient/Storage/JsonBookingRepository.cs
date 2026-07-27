using System.Text.Json;
using supClient.Models;

namespace supClient.Storage;

public class JsonBookingRepository : IBookingRepository
{
    readonly string _filePath;
    readonly SemaphoreSlim _lock = new(1, 1);

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public JsonBookingRepository()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, Defines.BookingsFileName);
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsByDateAsync(DateTime date)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await LoadAllAsync();
            return all
                .Where(b => b.StartTime.Date == date.Date)
                .OrderBy(b => b.StartTime)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await LoadAllAsync();
            return all.FirstOrDefault(b => b.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddBookingAsync(Booking booking)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await LoadAllAsync();
            all.Add(booking);
            await SaveAllAsync(all);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteBookingAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await LoadAllAsync();
            all.RemoveAll(b => b.Id == id);
            await SaveAllAsync(all);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await LoadAllAsync();
            var index = all.FindIndex(b => b.Id == booking.Id);
            if (index >= 0)
            {
                all[index] = booking;
                await SaveAllAsync(all);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAllBookingsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await SaveAllAsync([]);
        }
        finally
        {
            _lock.Release();
        }
    }

    async Task<List<Booking>> LoadAllAsync()
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var bookings = await JsonSerializer.DeserializeAsync<List<Booking>>(stream, JsonOptions);
            return bookings ?? [];
        }
        catch (JsonException)
        {
            BackupInvalidFile();
            return [];
        }
    }

    async Task SaveAllAsync(List<Booking> bookings)
    {
        EnsureStorageDirectoryExists();
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, bookings, JsonOptions);
    }

    void EnsureStorageDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }

    void BackupInvalidFile()
    {
        if (!File.Exists(_filePath))
            return;

        var backupPath = $"{_filePath}.invalid-{DateTime.UtcNow:yyyyMMddHHmmss}";
        File.Move(_filePath, backupPath, overwrite: true);
    }
}
