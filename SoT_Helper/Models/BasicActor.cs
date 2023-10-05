using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Models
{
    public struct BasicActor
    {
        public Coordinates Coords { get; set; }
        public float Distance { get; set; }
        public string RawName { get; set; }
        public int ActorId { get; set; }
        public ulong ActorAddress { get; set; }
    }
}
