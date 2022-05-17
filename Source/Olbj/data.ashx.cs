using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Olbj.Common;
using System.Data;

namespace Olbj
{
    /// <summary>
    /// data1 的摘要说明
    /// </summary>
    public class data : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            #region //需要参数传入
            var xcout = 10; //x轴均分数量
            var ycout = 10; //y轴均分数量
            var startx = 116.20;
            var endx = 116.54;
            var starty = 39.83;
            var endy = 40.03;

            try
            {
                xcout = Convert.ToInt32(context.Request.QueryString["xcout"]);
            }
            catch
            { }
            try
            {
                ycout = Convert.ToInt32(context.Request.QueryString["ycout"]);
            }
            catch
            { }
            try
            {
                startx = Convert.ToDouble(context.Request.QueryString["startx"]);
            }
            catch
            { }
            try
            {
                endx = Convert.ToDouble(context.Request.QueryString["endx"]);
            }
            catch
            { }
            try
            {
                starty = Convert.ToDouble(context.Request.QueryString["starty"]);
            }
            catch
            { }
            try
            {
                endy = Convert.ToDouble(context.Request.QueryString["endy"]);
            }
            catch
            { }
            #endregion

            Random rd = new Random();

            //分割正方向
            var xdistance = endx - startx;
            var ydistance = endy - starty;

            List<Rect> lisrCells=new List<Rect>();
            //构造小多边形
            for (var i = 0; i < xcout; i++) {
                for (var j = 0; j < ycout; j++) {

                    var startx_cell = startx+i*xdistance/xcout;
                    var endx_cell = startx + (i+1) * xdistance / xcout;
                    var starty_cell = starty + j * ydistance / ycout; ;
                    var endy_cell = starty + (j + 1) * ydistance / ycout;

                    Rect r = new Rect();
                    r.rectid = i + "-" + j;
                    r.x = startx_cell;
                    r.y = starty_cell;
                    r.height = ydistance / ycout;
                    r.width = xdistance / xcout;
                    lisrCells.Add(r);
                }
             }

            //开始计算权值
            Dictionary<string, ZonePower> dicpower = new Dictionary<string, ZonePower>();
            DataTable dtpoints = GPSInfoDal.Query("");
            string currentcarid=string.Empty;
            string currentzoneid=string.Empty;
            if (dtpoints != null && dtpoints.Rows.Count > 0)
            {
                for (int i = 0; i < dtpoints.Rows.Count; i++)
                {
                    double x=0;
                    double y=0;
                    try
                    {
                        x = Convert.ToDouble(dtpoints.Rows[i]["lon"]);
                    }
                    catch
                    { }
                    try
                    {
                        y = Convert.ToDouble(dtpoints.Rows[i]["lat"]);
                    }
                    catch
                    { }
                    string zoneid = getZoneid(lisrCells, x, y);
                    if (!currentcarid.Equals(dtpoints.Rows[i]["taxi_id"].ToString()))
                    {
                        currentcarid = dtpoints.Rows[i]["taxi_id"].ToString();
                        continue;
                    }
                    if (!string.IsNullOrEmpty(zoneid) && !string.IsNullOrEmpty(currentzoneid) && currentzoneid != zoneid)
                    {
                        string key = currentzoneid + "|" + zoneid;
                        if (!dicpower.ContainsKey(key))
                        {
                            ZonePower zonePower = new ZonePower();
                            zonePower.power = 1;
                            zonePower.startrectid = currentzoneid;
                            zonePower.endrectid = zoneid;
                            dicpower.Add(key, zonePower);
                        }
                        else
                        {
                            dicpower[key].power += 1;
                        }
                        currentzoneid = zoneid;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(currentzoneid) && !string.IsNullOrEmpty(zoneid))
                        {
                            currentzoneid = zoneid;
                        }
                    }
                }
            }

            //ZonePower 转zoneinfo
            List<ZonePower> listzonepower = dicpower.Values.ToList<ZonePower>();
            List<ZoneInfo> listzoneinfo = new List<ZoneInfo>();
            
            foreach(ZonePower power in listzonepower)
            {
                double rdl = (rd.NextDouble() - 0.5) / 3000;
                Rect start = getZone(lisrCells, power.startrectid);
                Rect end = getZone(lisrCells, power.endrectid);
                ZoneInfo info = new ZoneInfo();
                info.startx = start.x + start.width / 2 + rdl;
                info.starty = start.y + start.height / 2 + rdl;

                info.endx = end.x + end.width / 2 + rdl;
                info.endy = end.y + end.height / 2 + rdl;
                info.power = power.power;
                listzoneinfo.Add(info);
            }
            //在此写入您的处理程序实现。
            //string typestring = context.Request.QueryString["t"];
            string json = string.Empty;
            json = JsonConvert.SerializeObject(listzoneinfo);
            context.Response.Write(json);
        }

        public Rect getZone(List<Rect> lisrCells, string zoneid)
        {
            if (lisrCells != null & lisrCells.Count >= 0)
            {
                foreach (var cell in lisrCells)
                {
                    if (zoneid.Equals(cell.rectid))
                    {
                        return cell;
                    }
                }
            }
            return null;
        }
        public string getZoneid(List<Rect> lisrCells, double x, double y)
        {
            if (lisrCells != null & lisrCells.Count >= 0)
            {
                foreach (var cell in lisrCells)
                {
                    if (cell.ContainsPoint(x, y))
                    {
                        return cell.rectid;
                    }
                }
            }
            return string.Empty;
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class ZoneInfo
    {
        //开始区域
        public double startx
        {
            get;
            set;
        }

        public double starty
        {
            get;
            set;
        }
        //结束区域
        public double endx
        {
            get;
            set;
        }

        public double endy
        {
            get;
            set;
        }

        //权值
        public int power
        {
            get;
            set;
        }
    }

    public class ZonePower
    {
        //开始区域
        public string startrectid
        {
            get;
            set;
        }
        //结束区域
        public string endrectid
        {
            get;
            set;
        }
        //权值
        public int power
        {
            get;
            set;
        }
    }

    //长方形区域
    public class Rect
    {
        public string rectid
        {
            get;
            set;
        }

        public double x
        {
            get;
            set;
        }

        public double y
        {
            get;
            set;
        }

        public double width
        {
            get;
            set;
        }

        public double height
        {
            get;
            set;
        }

        public bool ContainsPoint(double px, double py)
        {
            if (px >= x && px <= x + width && py >= y && py <= y + height)
            {
                return true;
            }
            return false;
        }
    }
}