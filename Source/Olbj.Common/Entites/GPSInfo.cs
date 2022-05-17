using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olbj.Common.Entites
{
    public class GPSInfo
    {
        public string taxi_id
        {
            get;
            set;
        }
        public DateTime date_time
        {
            get;
            set;
        }
        public double lon
        {
            get;
            set;
        }
        public double lat
        {
            get;
            set;
        }
    }
}
