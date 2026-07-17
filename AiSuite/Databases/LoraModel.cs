using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using AiSuite.Models;

namespace AiSuite.Databases
{
    public class LoraModel
    {
        public LoraModel()
        {
        }

        public LoraModel(ModelFileItem modelFileItem)
        {
            ModelFilePath = modelFileItem.FilePath;
            BaseModel = modelFileItem.ModelMetadataDto.BaseModel;
            ModelName = modelFileItem.ModelMetadataDto.Model.Name;
            Description = modelFileItem.ModelMetadataDto.Model.Description;
            LastUpdatedAt = File.Exists(ModelFilePath) ? File.GetLastWriteTime(ModelFilePath) : DateTime.UtcNow;
            ThumbnailPath = modelFileItem.ThumbnailCachePath;
        }

        [Key]
        public int Id { get; set; }

        // .safetensors への絶対パス。その他の json や info へのパスの基準とする。
        public string ModelFilePath { get; set; }

        public string BaseModel { get; set; }

        public string Description { get; set; }

        public string ModelName { get; set; }

        public string ThumbnailPath { get; set; }

        // .safetensors の最終更新日時。差分取得の処理に使用する想定。
        public DateTime LastUpdatedAt { get; set; }
    }
}