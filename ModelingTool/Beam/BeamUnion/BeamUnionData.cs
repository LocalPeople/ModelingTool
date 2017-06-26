using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.BeamUnion
{
    class BeamUnionData
    {
        public bool CanUnion { get; set; }
        public FamilySymbol FamilySymbol { get; set; }
        public Line First { get; set; }
        public Level Level { get; set; }
        public ElementId MaterialId { get; set; }
        public string Message { get; set; }
        public Line Second { get; set; }
        public Dictionary<Guid, object> SharedParameters { get; set; }
    }
}
