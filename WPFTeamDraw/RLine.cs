using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFTeamDraw
{
    public struct RLine
    {

        public RLine(byte color, byte thickness) {
            this.color = color;
            this.thickness = thickness;
            this.servertime = Util.ServerTimeMillis();
            this.uid = Util.RandomLong();
        }

        public RLine(byte[] data)
        {
            color = data[0];
            thickness = data[1];
            uid = BitConverter.ToInt64(data, 2);
            servertime = BitConverter.ToInt64(data, 10);
        }

        public byte[] getBytes()
        {
            byte[] data = new byte[18];
            data[0] = color;
            data[1] = thickness;
            Array.Copy(BitConverter.GetBytes(uid), 0, data, 2, 8);
            Array.Copy(BitConverter.GetBytes(servertime), 0, data, 10, 8);
            return data;
        }

        public byte color;
        public byte thickness;
        public Int64 uid;
        public Int64 servertime;
    }
}
