using Autodesk.Revit.DB;
using ModelingTool.Interface;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.CutBeam
{
    class CutBeamData
    {
        List<XYZ> endPoints = new List<XYZ>();
        Line cutBeamLine;
        CutOperationMap operationMap;

        public FamilySymbol FamilySymbol { get; set; }
        public Level Level { get; set; }
        public ElementId MaterialId { get; set; }
        public XYZ[] Points
        {
            get
            {
                return endPoints.OrderBy(pt => pt, new AxisPointsComparer(cutBeamLine.Direction)).ToArray();
            }
        }
        public Dictionary<Guid, object> SharedParameters { get; set; }

        public CutBeamData(Document doc, FamilyInstance beam)
        {
            cutBeamLine = beam.ToLine();
            FamilySymbol = beam.Symbol;
            Level = doc.GetElement(beam.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).AsElementId()) as Level;
            MaterialId = beam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId();
            SharedParameters = GlobalUtil.GetSharedParameters(doc, beam);
            endPoints.Add(cutBeamLine.GetEndPoint(0));
            endPoints.Add(cutBeamLine.GetEndPoint(1));
            operationMap = new CutOperationMap();
        }

        public void Update(IEnumerable<Element> elements)
        {
            foreach (Element elem in elements)
            {
                Update(elem);
            }
        }

        public void Update(Element elem)
        {
            XYZ pt = operationMap.GetPoint(cutBeamLine, elem);
            if (pt != null)
            {
                endPoints.Add(pt);
                endPoints.Add(pt);
            }
        }
    }
}
