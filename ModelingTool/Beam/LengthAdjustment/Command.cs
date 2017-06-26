using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ModelingTool.Filter.Selection;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.LengthAdjustment
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            FamilyInstance[] beams = uiDoc.Selection.PickObjects(ObjectType.Element, BeamSelectionFilter.Single, "请选择需要处理的梁").Select(id => doc.GetElement(id) as FamilyInstance).ToArray();
            XYZ adjustFrom = uiDoc.Selection.PickPoint((ObjectSnapTypes)1023, "请选择端点");
            XYZ adjustTo = uiDoc.Selection.PickPoint((ObjectSnapTypes)1023, "请选择调整目标点");
            adjustFrom = adjustFrom - new XYZ(0, 0, adjustFrom.Z);
            adjustTo = adjustTo - new XYZ(0, 0, adjustTo.Z);

            using (Transaction trans = new Transaction(doc, "批量调整梁端点"))
            {
                trans.Start();
                adjustBeamLength(doc, beams, adjustFrom, adjustTo);
                trans.Commit();
            }

            return Result.Succeeded;
        }

        private void adjustBeamLength(Document doc, IEnumerable<FamilyInstance> beams, XYZ adjustFrom, XYZ adjustTo)
        {
            foreach (FamilyInstance beam in beams)
            {
                LocationCurve locationCurve = beam.Location as LocationCurve;
                double end0ToAdjustFrom = locationCurve.Curve.GetEndPoint(0).DistanceTo(adjustFrom);
                double end1ToAdjustFrom = locationCurve.Curve.GetEndPoint(1).DistanceTo(adjustFrom);
                XYZ direction = (locationCurve.Curve as Line).Direction;
                Line longLine = Line.CreateBound(beam.ToLine().GetEndPoint(0) - direction * 100, beam.ToLine().GetEndPoint(1) + direction * 100);
                XYZ ptStart, ptEnd;
                if (end0ToAdjustFrom > end1ToAdjustFrom)
                {
                    ptStart = beam.ToLine().GetEndPoint(0);
                    ptEnd = longLine.Project(adjustTo).XYZPoint;
                }
                else
                {
                    ptStart = longLine.Project(adjustTo).XYZPoint;
                    ptEnd = beam.ToLine().GetEndPoint(1);
                }
                locationCurve.Curve = Line.CreateBound(ptStart, ptEnd);
            }
        }
    }
}
