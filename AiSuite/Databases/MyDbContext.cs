using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AiSuite.Models;
using AiSuite.Utils;
using Microsoft.EntityFrameworkCore;

namespace AiSuite.Databases
{
    public class MyDbContext : DbContext
    {
        public DbSet<LoraModel> LoraModels { get; set; }

        /// <summary>
        /// 入力された modelFiles を 内部で LoraModel に変換して登録します。
        /// modelFiles は変更されません。
        /// </summary>
        /// <param name="modelFiles">登録したい modelFiles</param>
        /// <returns> 非同期を表す Task　</returns>
        public async Task AddRangeAsync(IEnumerable<ModelFileItem> modelFiles)
        {
            // 追加したいリスト
            var newModels = modelFiles.Select(m => new LoraModel(m)).ToList();

            // 1. 追加したいリストの中のパスを抽出
            var newFilePaths = newModels.Select(m => m.ModelFilePath).ToList();

            // 2. DBに既に存在するパスを特定する
            var existingPaths = LoraModels
                .Where(m => newFilePaths.Contains(m.ModelFilePath))
                .Select(m => m.ModelFilePath)
                .ToHashSet();

            // 3. 存在しないヤツだけリストに残す
            var modelsToAdd = newModels.Where(m => !existingPaths.Contains(m.ModelFilePath)).ToList();

            // 4. 重複なしのアイテムだけ追加
            if (modelsToAdd.Any())
            {
                LoraModels.AddRange(modelsToAdd);
                await SaveChangesAsync();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var baseDir = AppContext.BaseDirectory;
                var dbPath = Path.Combine(baseDir, "browse_data.db");
                Logger.Log($"DB Path: {dbPath}");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}