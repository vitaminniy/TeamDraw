using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFTeamDraw
{
    public struct RPoint
    {
        public RPoint(double x, double y, Int64 uid)
        {
            this.x = x;
            this.y = y;
            this.uid = uid;
        }

        public RPoint(byte[] data)
        {
            x = BitConverter.ToDouble(data, 0);
            y = BitConverter.ToDouble(data, 8);
            uid = BitConverter.ToInt64(data, 16);
        }

        public byte[] getBytes()
        {
            byte[] data = new byte[24];
            Array.Copy(BitConverter.GetBytes(x), 0, data, 0, 8);
            Array.Copy(BitConverter.GetBytes(y), 0, data, 8, 8);
            Array.Copy(BitConverter.GetBytes(uid), 0, data, 16, 8);
            return data;
        }

        public double x;
        public double y;
        public Int64 uid;
    }
}
