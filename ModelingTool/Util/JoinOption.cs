using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Util
{
    class JoinOption
    {
        public bool JoinByBeam { get; set; }
        public bool JoinByWall { get; set; }
        public bool JoinByFloor { get; set; }
        public bool JoinByColumn { get; set; }
    }
}
