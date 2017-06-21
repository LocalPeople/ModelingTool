using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ModelingTool.Filter.Selection;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.BeamUnion
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            while (true)
            {
                try
                {
                    Reference beamRefer1 = uiDoc.Selection.PickObject(ObjectType.Element, BeamSelectionFilter.Single, "请选择需要合并的第一条梁");
                    Reference beamRefer2 = uiDoc.Selection.PickObject(ObjectType.Element, BeamSelectionFilter.Single, "请选择需要合并的第二条梁");
                    FamilyInstance Beam1 = doc.GetElement(beamRefer1) as FamilyInstance;
                    FamilyInstance Beam2 = doc.GetElement(beamRefer2) as FamilyInstance;

                    using (Transaction trans = new Transaction(doc, "合并梁"))
                    {
                        trans.Start();
                        BeamUnionData data = Validation(doc, Beam1, Beam2);
                        if (!data.CanUnion)
                        {
                            throw new InvalidOperationException(data.Message);
                        }
                        Union(doc, Beam1, Beam2, data);
                        trans.Commit();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    TaskDialog.Show("Revit", ex.Message);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    break;
                }
            }

            return Result.Succeeded;
        }

        private void Union(Document doc, FamilyInstance beam1, FamilyInstance beam2, BeamUnionData data)
        {
            doc.Delete(beam1.Id);
            doc.Delete(beam2.Id);
            doc.Regenerate();

            XYZ beamStart, beamEnd;
            if (data.First.Direction.IsAlmostEqualTo(data.Second.Direction))
            {
                double beamLength1 = data.First.GetEndPoint(0).DistanceTo(data.Second.GetEndPoint(1));
                double beamLength2 = data.First.GetEndPoint(1).DistanceTo(data.Second.GetEndPoint(0));
                beamStart = beamLength1 > beamLength2 ? data.First.GetEndPoint(0) : data.First.GetEndPoint(1);
                beamEnd = beamLength1 > beamLength2 ? data.Second.GetEndPoint(1) : data.Second.GetEndPoint(0);
            }
            else
            {
                double beamLength1 = data.First.GetEndPoint(0).DistanceTo(data.Second.GetEndPoint(0));
                double beamLength2 = data.First.GetEndPoint(1).DistanceTo(data.Second.GetEndPoint(1));
                beamStart = beamLength1 > beamLength2 ? data.First.GetEndPoint(0) : data.First.GetEndPoint(1);
                beamEnd = beamLength1 > beamLength2 ? data.Second.GetEndPoint(0) : data.Second.GetEndPoint(1);
            }

            FamilyInstance newBeam = doc.Create.NewFamilyInstance(Line.CreateBound(beamStart, beamEnd), data.FamilySymbol, data.Level, StructuralType.Beam);
            GlobalUtil.SetSharedParameters(newBeam, data.SharedParameters);
            IList<Element> inclusion = GlobalUtil.IncludeTest(doc, newBeam);
            JoinManager.OfBeam(doc, newBeam, inclusion);
        }

        private BeamUnionData Validation(Document doc, FamilyInstance beam1, FamilyInstance beam2)
        {
            BeamUnionData data = new BeamUnionData();
            if (beam1.Id == beam2.Id)
            {
                data.CanUnion = false;
                data.Message = "两次选择的梁相同";
            }
            else
            {
                double height1, width1, height2, width2;
                GlobalUtil.GetBeamDimension(beam1, out height1, out width1);
                GlobalUtil.GetBeamDimension(beam2, out height2, out width2);
                Line centerLine1 = GetBeamCenterLine(beam1, width1);
                Line centerLine2 = GetBeamCenterLine(beam2, width2);

                XYZ compare = centerLine1.Direction;
                XYZ tmpVector1 = (centerLine1.GetEndPoint(0) - centerLine2.GetEndPoint(0)).Normalize();
                XYZ tmpVector2 = (centerLine1.GetEndPoint(0) - centerLine2.GetEndPoint(1)).Normalize();
                XYZ tmpVector3 = (centerLine1.GetEndPoint(1) - centerLine2.GetEndPoint(0)).Normalize();
                XYZ tmpVector4 = (centerLine1.GetEndPoint(1) - centerLine2.GetEndPoint(1)).Normalize();

                int passCount = 0;
                passCount = tmpVector1.IsAlmostEqualTo(compare) || tmpVector1.IsAlmostEqualTo(-compare) ? passCount + 1 : passCount;
                passCount = tmpVector2.IsAlmostEqualTo(compare) || tmpVector2.IsAlmostEqualTo(-compare) ? passCount + 1 : passCount;
                passCount = tmpVector3.IsAlmostEqualTo(compare) || tmpVector3.IsAlmostEqualTo(-compare) ? passCount + 1 : passCount;
                passCount = tmpVector4.IsAlmostEqualTo(compare) || tmpVector4.IsAlmostEqualTo(-compare) ? passCount + 1 : passCount;
                if (passCount >= 3)
                {
                    data.CanUnion = true;
                    data.First = centerLine1;
                    data.Second = centerLine2;
                    data.FamilySymbol = beam1.Symbol;
                    data.Level = doc.GetElement(beam1.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).AsElementId()) as Level;
                    data.SharedParameters = GlobalUtil.GetSharedParameters(doc, beam1);
                }
                else
                {
                    data.CanUnion = false;
                    data.Message = "需要合并的两条梁不在同一直线上";
                }
            }

            return data;
        }

        private Line GetBeamCenterLine(FamilyInstance beam, double width)
        {
            Line RealLine = (beam.Location as LocationCurve).Curve as Line;
            Transform transform = Transform.Identity;
            switch (beam.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).AsValueString())
            {
                case "原点":
                case "中心线":
                    break;
                case "左":
                    XYZ rightMove = new XYZ(RealLine.Direction.Y, -RealLine.Direction.X, 0);
                    transform = Transform.CreateTranslation(rightMove * width * 0.5);
                    break;
                case "右":
                    XYZ leftMove = new XYZ(-RealLine.Direction.Y, RealLine.Direction.X, 0);
                    transform = Transform.CreateTranslation(leftMove * width * 0.5);
                    break;
            }
            return RealLine.CreateTransformed(transform) as Line;
        }
    }
}
