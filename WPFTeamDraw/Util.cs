using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFTeamDraw
{
    public class Util
    {
        private static readonly Random r = new Random();
        private static readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long _serverTimeDifference = 0;

        public static long ServerTimeDifference
        {
            get { return _serverTimeDifference; }
            set { _serverTimeDifference = value; }
        }

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public static long ServerTimeMillis()
        {
            return CurrentTimeMillis() + ServerTimeDifference;
        }

        public static long RandomLong()
        {
            return (long)((r.NextDouble() * 2.0 - 1.0) * long.MaxValue);
        }
    }
}
