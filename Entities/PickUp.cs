using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vantex.Technician.Service.Entities
{
    public class PickUp : ICoordinate
    {
        public int OrderDetailId { get; set; }
        public string UnitID { get; set; }
        public string Easting { get; set; }
        public string Northing { get; set; }
        public DateTime PUDate { get; set; }
        public string PUCompleted { get; set; }
        public string Note { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public string PartPUMemo { get; set; }
        public string PUTime { get; set; }
        public string ReqNumber { get; set; }
        public string OrderNumber { get; set; }
        public string Pickup { get; set; }
        public string PartPU { get; set; }

        public int MonRN { get; set; }
        public int MonRO { get; set; }

        public int TueRN { get; set; }
        public int TueRO { get; set; }

        public int WedRN { get; set; }
        public int WedRO { get; set; }

        public int ThuRN { get; set; }
        public int ThuRO { get; set; }

        public int FriRN { get; set; }
        public int FriRO { get; set; }

        public int SatRN { get; set; }
        public int SatRO { get; set; }

        public int SunRN { get; set; }
        public int SunRO { get; set; }

        public int Mon { get; set; }
        public int Tue { get; set; }
        public int Wed { get; set; }
        public int Thu { get; set; }
        public int Fri { get; set; }
        public int Sat { get; set; }
        public int Sun { get; set; }
    }
}
