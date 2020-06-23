using CoordinateSharp;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Vantex.Technician.Service.Entities;
using Vantex.Technician.Service.Helpers;
using Vantex.Technician.Service.Services;

namespace Vantex.Technician.Service.Repositories
{
    public class ScheduleRepository : BaseRepository
    {
        public ScheduleRepository(IOptions<AppSettings> appSettings) : base(appSettings)
        {
        }

        public async Task<IEnumerable<object>> GetAllTerminals(int baseId)
        {
            var query = $"SELECT * FROM qryTerminals WHERE BaseID = {baseId} AND Button NOT LIKE '%-%' AND Lat IS NOT NULL AND Lon IS NOT NULL";
            return await SelectAsync<object>(query, null);
        }

        #region Deliveries
        public async Task<IEnumerable<Delivery>> GetAllDeliveries(int baseId)
        {
            var query = "SELECT OrderDetailId, UnitType, Customer, Area, Description, PUDate, Note1 as Note, Qty as Quantity, East as Lat, North as Lon " +
                        "FROM qryNewDeliveries " +
                       $"WHERE BaseID = {baseId} " +
                          @"AND ((DelCompleted = 0 AND PUCompleted = 0) 
                            OR (Replace = 1 AND PUCompleted = 0) 
                            OR (Relocate = 1)) 
                            AND (Description NOT LIKE '%SERVICE HOLDING%') 
                        ORDER by QUOTE ASC, OrderNumber, DelTime, DelDate, OrderDetailID";

            return await SelectAsync<Delivery>(query, new { });
        }
        #endregion

        #region PickUps
        public async Task<IEnumerable<PickUp>> GetAllPickUps(int baseId)
        {
            if (await BaseIdExist(baseId))
            {
                PickUpInput input = new PickUpInput(baseId, DateTime.Today.Date, await GetShortBase(baseId));

                List<PickUp> pickUps = (List<PickUp>)(await GetPickUpData(input));
                if (pickUps == null)
                {
                    return new List<PickUp>();
                }

                return pickUps;
            }
            return null;

        }

        private async Task<string> GetShortBase(int baseId)
        {
            return await GetAsync<string>("select ShortBase from tblBases where BaseID = @baseId", new { baseId });
        }

        public async Task<IEnumerable<PickUp>> GetPickUpData(PickUpInput input)
        {
            string query = "SELECT * " +
                            "FROM qryInventory " +
                            "WHERE PartPU = 1 " +
                                $"AND PartPUDate <= '{input.Today:yyyy-MM-dd}' " +
                                $"AND BaseID = {input.BaseId} " +
                                "OR Pickup = 1 " +
                                $"AND BaseID = {input.BaseId} " +
                                $"OR PUDate <= '{input.Today:yyyy-MM-dd}' " +
                                "AND PUDate > '1/1/2000' " +
                                "AND PUCompleted = 0 " +
                                $"AND BaseID = {input.BaseId}";

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    db.Open();

                    return await db.QueryAsync<PickUp>(query);
                }
            }
            catch (Exception)
            {
                return null;
            }

        }
        #endregion

        #region Services
        public async Task<IEnumerable<ServiceEntity>> GetAllServices(int baseId)
        {
            ServiceInput input = new ServiceInput(baseId, DateTime.Today.Date, await GetShortBase(baseId));
            List<ServiceEntity> serviceData = (List<ServiceEntity>)(await GetServiceData(input));

            if (serviceData == null)
            {
                return new List<ServiceEntity>();
            }
            await UpdateServiceData(serviceData, input);

            return serviceData;
        }

        private async Task UpdateServiceData(List<ServiceEntity> serviceData, ServiceInput input)
        {
            foreach (ServiceEntity serviceDataItem in serviceData)
            {
                if (double.TryParse(serviceDataItem.Lat, out double latitude) && double.TryParse(serviceDataItem.Lon, out double longitude))
                {
                    serviceDataItem.MGRS = ConvertToMGRS(latitude, longitude);
                }

                SetGeographicCoordinates(serviceDataItem);
                string weekDay = input.GetSvcDay();
                serviceDataItem.MonRN = GetValueOf(weekDay, "RN", serviceDataItem);
                serviceDataItem.MonRO = GetValueOf(weekDay, "RO", serviceDataItem);
                if (Convert.ToBoolean(serviceDataItem.Pickup))
                {
                    serviceDataItem.MonRN = 1000; //Do not service
                    continue;
                }
                else
                {
                    if (serviceDataItem.PUDate != null)
                    {
                        if (serviceDataItem.PUDate.Date > new DateTime(1900, 12, 30).Date && serviceDataItem.PUDate.Date <= input.Today.Date)
                        {
                            serviceDataItem.MonRN = 1000; //Do not service
                            continue;
                        }
                    }
                    if (serviceDataItem.DelDate != null)
                    {
                        if (serviceDataItem.DelDate.Date > new DateTime(1900, 12, 30).Date && serviceDataItem.DelDate.Date >= input.Today.Date)
                        {
                            serviceDataItem.MonRN = 1000; //Do not service
                            continue;
                        }
                    }
                    if (serviceDataItem.SvcFreq != null)
                    {
                        switch (serviceDataItem.SvcFreq)
                        {
                            case ServiceFrequency.EveryOtherDay:
                                if (await SetUpMonRNorSvcFreq(serviceDataItem, input, ServiceFrequency.EveryOtherDay))
                                {
                                    continue;
                                }
                                break;
                            case ServiceFrequency.BiWeekly:
                                if (await SetUpMonRNorSvcFreq(serviceDataItem, input, ServiceFrequency.BiWeekly))
                                {
                                    continue;
                                }
                                break;
                            case ServiceFrequency.Monthly:
                                if (await SetUpMonRNorSvcFreq(serviceDataItem, input, ServiceFrequency.Monthly))
                                {
                                    continue;
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    if (serviceDataItem.MonRN == 0)
                    {
                        serviceDataItem.MonRN = 99;
                    }
                    UpdateOrderNumberBasedOnShortBaseAndReqNumberValue(input, serviceDataItem);
                }
            }
        }

        private async Task<bool> SetUpMonRNorSvcFreq(ServiceEntity item, ServiceInput input, string mode)
        {
            int svcFreq = 0;
            int svcGroup = await GetSvcGroup(item.Button);
            CultureInfo myCI = new CultureInfo("en-US");
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            Calendar myCal = myCI.Calendar;

            switch (mode)
            {
                case ServiceFrequency.EveryOtherDay:
                    svcFreq = input.Today.DayOfYear % 2;
                    break;
                case ServiceFrequency.BiWeekly:
                    svcFreq = myCal.GetWeekOfYear(DateTime.Now, myCWR, input.Today.DayOfWeek) % 2;
                    break;
                case ServiceFrequency.Monthly:
                    svcFreq = myCal.GetWeekOfYear(DateTime.Now, myCWR, input.Today.DayOfWeek) % 4;
                    break;
                default:
                    break;
            }

            if (svcGroup != svcFreq)
            {
                item.MonRN = 1000;
                return true; //breaks one iteration (in the loop)
            }
            item.SvcFreq += $"-{svcFreq}";
            return false;
        }

        private async Task<int> GetSvcGroup(string buttonID)
        {
            int svcGroup = 99;
            string ifSvcGroupExistQuery = "SELECT COUNT(SvcGroup) FROM tblButtons WHERE Button LIKE @buttonID";
            if (await GetAsync<int>(ifSvcGroupExistQuery, new { buttonID }) == 1)
            {
                svcGroup = await GetAsync<int>("SELECT SvcGroup FROM tblButtons WHERE Button LIKE @buttonID", new { buttonID });
            }

            return svcGroup;
        }

        //return a value of a property dynamically
        private int GetValueOf(string weekDay, string objectPropertyPostfixName, ServiceEntity item)
        {
            string objectPropertyName = weekDay + objectPropertyPostfixName;
            return (int)(item.GetType().GetProperty(objectPropertyName).GetValue(item, null));
        }

        public string GetServiceQuery(string shortBase)
        {
            string query = @"SELECT 
                                ReqNumber, OrderDetailId, OrderID, UnitId, UnitType, csTable.OrderNumber, PUdate, DelDate, 
                                Easting, Northing, SvcFreq, Lon, Lat, Button, PickUp, 
                                Qty AS Quantity, Customer, Area, Description, ServiceNote, SSubdivison, GridSquare, 
                                M AS Mon, Tu AS Tue, W AS Wed, Th AS Thu, F AS Fri, Sa as Sat, Su AS Sun, 
                                TuRN AS TueRN, TuRO AS TueRO, WRN AS WedRN, MRN AS MonRN, MRO AS MonRO, WRO AS WedRO, 
                                ThRN AS ThuRN, ThRO AS ThuRO, FRN AS FriRN, FRO AS FriRO, 
                                SaRN AS SatRN, SaRO AS SatRO, SuRN AS SunRN, SuRO AS SunRO, 
                                tblOrders.ExtOrderID, Manufacturer 
                            FROM tblCompressedSchedule AS csTable
                            INNER JOIN tblOrders ON csTable.OrderNumber = tblOrders.OrderNumber ";

            if (shortBase == "Pendleton")
            {
                return query += "ORDER BY area, gridsquare, easting, northing";
            }

            return query += "ORDER BY deldate, area, gridsquare, easting, northing";
        }
        public async Task<IEnumerable<ServiceEntity>> GetServiceData(ServiceInput input)
        {
            const string STORE_PROCEDURE_NAME = "dbo.spu_ServiceSchedule";

            try
            {
                using (IDbConnection db = GetConnection())
                {
                    var values = new { BaseID = input.BaseId, RptDate = input.Today, SvcDay = GetSvcDayForStoreProcedure(input.GetSvcDay()) };
                    await db.QueryAsync(STORE_PROCEDURE_NAME, values, commandType: CommandType.StoredProcedure);
                    return await db.QueryAsync<ServiceEntity>(GetServiceQuery(input.ShortBase));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetSvcDayForStoreProcedure(string svcDay)
        {
            return (svcDay.Substring(0, 1) == "M" || svcDay.Substring(0, 1) == "W" || svcDay.Substring(0, 1) == "F")
                    ? svcDay.Substring(0, 1) : svcDay.Substring(0, 2);
        }

        private string ConvertToMGRS(double latitude, double longitude)
        {
            Coordinate coordinate = new Coordinate(latitude, longitude);
            return $"{coordinate.MGRS.Digraph} {coordinate.MGRS.Easting} {coordinate.MGRS.Northing}";
        }

        #endregion

        #region PickUps and Services Common

        private void SetGeographicCoordinates(ICoordinate item)
        {
            if (item.Easting != null)
            {
                item.Lat = item.Easting;
            }
            else
            {
                item.Lat = "No Grid";
            }

            if (item.Northing != null)
            {
                item.Lon = item.Northing;
            }
            else
            {
                item.Lon = string.Empty;
            }
        }

        private bool UpdateOrderNumberBasedOnShortBaseAndReqNumberValue(dynamic input, dynamic item)
        {
            if (input.ShortBase == "Polk" && item.ReqNumber != null)
            {
                item.OrderNumber = item.ReqNumber;
                return true;
            }
            return false;
        }

        public async Task<bool> BaseIdExist(int baseId)
        {
            string ifBaseIdExistQuery = $"SELECT COUNT(BaseID) FROM tblBases WHERE BaseID = @baseId";
            if (await GetAsync<int>(ifBaseIdExistQuery, new { baseId }) == 1)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
