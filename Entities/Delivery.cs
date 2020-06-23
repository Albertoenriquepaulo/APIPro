using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vantex.Technician.Service.Entities
{
    public class Delivery
    {
        public int OrderDetailId { get; set; }
        public int Quantity { get; set; }
        public string UnitType { get; set; }
        public string Customer { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public DateTime PUDate { get; set; }
    }
}
