﻿using MemoRandom.Data.DbModels;
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
