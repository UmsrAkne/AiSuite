using System;
using System.IO;
using System.Text.Json;
using AiSuite.Models.DTOs;

namespace AiSuite.Utils
{
    public class ModelMetadataParser
    {
        private readonly static JsonSerializerOptions Options = new ()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static T ParseJsonFile<T>(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    return default;
                }

                var jsonString = File.ReadAllText(jsonFilePath);
                return JsonSerializer.Deserialize<T>(jsonString, Options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Jsonパース失敗: {ex.Message}");
                return default;
            }
        }
    }
}