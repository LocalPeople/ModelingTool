using Autodesk.Revit.DB;
using ModelingTool.Interface;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.BeamUnion
{
    class BeamUnionData
    {
        List<XYZ> endPoints = new List<XYZ>();
        Line lastBeamLine;

        public bool CanUnion { get; set; }
        public XYZ End
        {
            get
            {
                return endPoints.OrderBy(pt => pt, new AxisPointsComparer(lastBeamLine.Direction)).Last();
            }
        }
        public FamilySymbol FamilySymbol { get; set; }
        public Level Level { get; set; }
        public ElementId MaterialId { get; set; }
        public string Message { get; set; }
        public XYZ Start
        {
            get
            {
                return endPoints.OrderBy(pt => pt, new AxisPointsComparer(lastBeamLine.Direction)).First();
            }
        }
        public Dictionary<Guid, object> SharedParameters { get; set; }

        public bool Update(Document doc, FamilyInstance beam)
        {
            if (FamilySymbol == null)
            {
                FamilySymbol = beam.Symbol;
            }
            if (Level == null)
            {
                Level = doc.GetElement(beam.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).AsElementId()) as Level;
            }
            if (MaterialId == null)
            {
                MaterialId = beam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId();
            }
            if (SharedParameters == null)
            {
                SharedParameters = GlobalUtil.GetSharedParameters(doc, beam);
            }

            Line real = GlobalUtil.GetBeamCenterLine(beam);
            if (lastBeamLine == null)
            {
                lastBeamLine = real;
                endPoints.Add(real.GetEndPoint(0));
                endPoints.Add(real.GetEndPoint(1));
                CanUnion = true;
            }
            else
            {
                if (GlobalUtil.Collineation(lastBeamLine, real))
                {
                    lastBeamLine = real;
                    endPoints.Add(real.GetEndPoint(0));
                    endPoints.Add(real.GetEndPoint(1));
                    CanUnion = true;
                }
                else
                {
                    CanUnion = false;
                    Message = "需要合并的梁不在同一直线上";
                }
            }

            return CanUnion;
        }
    }
}
