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

        public static ModelMetadataDto ParseJsonFile(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    return null; // ファイルがないので中断。
                }

                var jsonString = File.ReadAllText(jsonFilePath);
                return JsonSerializer.Deserialize<ModelMetadataDto>(jsonString, Options);
            }
            catch (Exception ex)
            {
                // ログ
                System.Diagnostics.Debug.WriteLine($"Jsonパース失敗: {ex.Message}");
                return null;
            }
        }

        public static CivitaiHelperInfoDto ParseCivitaiHelperJsonFile(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    return null; // ファイルがないので中断。
                }

                var jsonString = File.ReadAllText(jsonFilePath);
                return JsonSerializer.Deserialize<CivitaiHelperInfoDto>(jsonString, Options);
            }
            catch (Exception ex)
            {
                // ログ
                System.Diagnostics.Debug.WriteLine($"Jsonパース失敗: {ex.Message}");
                return null;
            }
        }
    }
}