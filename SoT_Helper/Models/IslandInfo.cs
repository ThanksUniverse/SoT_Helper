using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Models
{
    public class IslandInfo
    {
        public Image Image { get; set; }
        public string Rawname { get; set; }
        public PointF Position { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }
    }
}
