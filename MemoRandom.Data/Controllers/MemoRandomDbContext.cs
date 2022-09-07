using System.Collections.Immutable;
using MemoRandom.Data.DbModels;
using Microsoft.EntityFrameworkCore;

namespace MemoRandom.Data.Controllers
{
    public class MemoRandomDbContext : DbContext
    {
        private readonly string _connectionString; // Строка подключения к БД

        /// <summary>
        /// Набор справочника причин смерти
        /// </summary>
        public DbSet<DbReason> DbReasons { get; set; }

        /// <summary>
        /// Набор характеристик людей
        /// </summary>
        public DbSet<DbHuman> DbHumans { get; set; }

        /// <summary>
        /// Набор категорий возрастных периодов
        /// </summary>
        public DbSet<DbCategory> DbCategories { get; set; }

        /// <summary>
        /// Набор людей для сравнения
        /// </summary>
        public DbSet<DbComparedHuman> DbComparedHumans { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            if (!builder.IsConfigured)
            {
                builder.UseSqlServer(_connectionString);
            }
        }

        #region CTOR
        public MemoRandomDbContext(string connectionString)
        {
            _connectionString = connectionString;
            Database.EnsureCreated();
        }
        #endregion
    }
}
