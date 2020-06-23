using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Vantex.Technician.Service.Helpers;

namespace Vantex.Technician.Service.Repositories
{
    public abstract class BaseRepository
    {

        protected readonly AppSettings _appSettings;
        protected internal static class Legacy
        {
            internal const string True = "1";
            internal const string False = "0";
        }

        protected BaseRepository(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_appSettings.VantexDatabaseConnectionString);
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sqlString)
        {
            using (IDbConnection db = GetConnection())
            {
                var list = await db.QueryAsync<T>(sqlString);
                return list;
            }
        }

        public async Task<T> CommandAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> command)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var result = await command(connection, transaction);

                        transaction.Commit();

                        return result;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task<T> GetAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> command)
        {
            return await CommandAsync(command);
        }

        public async Task<IList<T>> SelectAsync<T>(Func<SqlConnection, SqlTransaction, Task<IList<T>>> command)
        {
            return await CommandAsync(command);
        }

        public async Task ExecuteAsync(string sql, object parameters)
        {
            await CommandAsync(async (conn, trn) =>
            {
                await conn.ExecuteAsync(sql, parameters, trn);
                return 1;
            });
        }

        public async Task<T> GetAsync<T>(string sql, object parameters)
        {

            return await CommandAsync(async (conn, trn) =>
            {
                T result = await conn.QuerySingleAsync<T>(sql, parameters, trn);
                return result;
            });
        }

        public async Task<IList<T>> SelectAsync<T>(string sql, object parameters)
        {
            return await CommandAsync<IList<T>>(async (conn, trn) =>
            {
                var result = (await conn.QueryAsync<T>(sql, parameters, trn)).ToList();
                return result;
            });
        }
    }
}
