using booking_service.Models;

namespace booking_service.Services;

public class EventServiceUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

public class EventServiceClient(HttpClient http)
{
    /// <summary>Returns null when the event is unknown. Throws EventServiceUnavailableException when unreachable.</summary>
    public async Task<EventInfo?> GetEventAsync(Guid id, CancellationToken ct)
    {
        try
        {
            using var response = await http.GetAsync($"api/events/{id}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

            if (!response.IsSuccessStatusCode)
                throw new EventServiceUnavailableException($"event-service returned {(int)response.StatusCode}.");

            return await response.Content.ReadFromJsonAsync<EventInfo>(ct)
                   ?? throw new EventServiceUnavailableException("event-service returned an empty body.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException && !ct.IsCancellationRequested)
        {
            throw new EventServiceUnavailableException("event-service is unreachable.", ex);
        }
    }
}
