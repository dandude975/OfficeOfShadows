// OOS.Shared/GameMessage.cs
using System;
using System.Text.Json;

namespace OOS.Shared
{
    public record GameMessage
    {
        public string Type { get; init; } = "";      // e.g., "device.mark_suspicious"
        public string From { get; init; } = "";      // e.g., "DeviceManager"
        public object? Data { get; init; }           // anonymous payload or small DTO
        public DateTime Utc { get; init; } = DateTime.UtcNow;

        public static GameMessage FromJson(string json)
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<GameMessage>(json, opts)
                   ?? new GameMessage { Type = "invalid", From = "deserialize" };
        }

        public string ToJson()
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            return JsonSerializer.Serialize(this, opts);
        }
    }
}
