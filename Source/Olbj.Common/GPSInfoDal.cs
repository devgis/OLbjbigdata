using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Olbj.Common.Entites;
using System.Data.SqlClient;

namespace Olbj.Common
{
    public class GPSInfoDal
    {
        public static DataTable Query(string where)
        {
            string sql = "select * from t_GPSInfo order by taxi_id,date_time";
            if (!string.IsNullOrEmpty(where))
            {
                sql += " where " + where;
            }
            return SQLHelper.Instance.GetDataTable(sql);
        }
    }
}
