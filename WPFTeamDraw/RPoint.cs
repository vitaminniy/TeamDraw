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
        Point point;
        long servertime;
        long uid;
        bool isFinal;
    }
}
