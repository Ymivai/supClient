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
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonBookingRepository()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, Defines.BookingsFileName);
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsByDateAsync(DateTime date)
    {
        var all = await LoadAllAsync();
        return all
            .Where(b => b.StartTime.Date == date.Date)
            .OrderBy(b => b.StartTime)
            .ToList();
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

    async Task<List<Booking>> LoadAllAsync()
    {
        if (!File.Exists(_filePath))
            return [];

        await using var stream = File.OpenRead(_filePath);
        var bookings = await JsonSerializer.DeserializeAsync<List<Booking>>(stream, JsonOptions);
        return bookings ?? [];
    }

    async Task SaveAllAsync(List<Booking> bookings)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, bookings, JsonOptions);
    }
}
