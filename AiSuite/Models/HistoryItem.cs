using System;

namespace AiSuite.Models
{
    public enum HistoryType
    {
        ImageLoad,
        ImageReload,
        SelectionConfirmed,
    }

    public class HistoryItem
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public HistoryType Type { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Description} - {Detail}";
        }
    }
}