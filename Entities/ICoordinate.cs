using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vantex.Technician.Service.Entities
{
    public interface ICoordinate
    {
        string Easting { get; set; }
        string Northing { get; set; }

        string Lon { get; set; }
        string Lat { get; set; }
    }
}
