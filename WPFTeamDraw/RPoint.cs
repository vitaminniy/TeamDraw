using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFTeamDraw
{
    [Serializable]
    public struct RPoint
    {
        double x;
        double y;
        Int64 servertime;
        Int64 uid;
        byte isFinal;
    }
}
