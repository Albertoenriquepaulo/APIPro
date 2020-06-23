using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Vantex.Technician.Service.Entities;
using Vantex.Technician.Service.Helpers;

namespace Vantex.Technician.Service.Repositories
{

    public class UserRepository : BaseRepository
    {

        const string getQuery = @"
            SELECT TOP (1000) 
                ProbeNameTable.[ProbeIndex] AS Id,
                ProbeNameTable.[BaseID],
                BasesTable.[BaseName],
                ProbeNameTable.[ProbeID],
                ProbeNameTable.[ProbeName],
                ProbeNameTable.[TechName] AS Technician,
                ProbeNameTable.[TrackMe]
            FROM [dbo].[tblProbeNames] AS ProbeNameTable
            INNER JOIN tblBases AS BasesTable 
            ON ProbeNameTable.BaseID = BasesTable.BaseID";

        public UserRepository(IOptions<AppSettings> appSettings) : base(appSettings)
        {
        }

        public IEnumerable<User> GetByProbeId(string probeId, string userPass)
        {
            List<User> users;
            using (IDbConnection db = new SqlConnection(_appSettings.VantexDatabaseConnectionString))
            {
                users = db.Query<User>($"{getQuery} WHERE PROBEID = '{probeId}' AND PASSWORD = '{userPass}'").ToList();
            }
            return CleansePasswords(users);
        }

        public IEnumerable<User> GetAll()
        {
            IEnumerable<User> users;
            using (IDbConnection db = new SqlConnection(_appSettings.VantexDatabaseConnectionString))
            {
                users = db.Query<User>(getQuery);
            }
            return CleansePasswords(users);
        }

        private IEnumerable<User> CleansePasswords(IEnumerable<User> users)
        {
            return users.Select(x =>
            {
                x.Password = null;
                return x;
            });
        }
    }
}
