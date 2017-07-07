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

namespace ModelingTool.Beam.CutBeam
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
                    Reference beamRefer = uiDoc.Selection.PickObject(ObjectType.Element, BeamSelectionFilter.Single, "请选择需要拆分的梁");
                    FamilyInstance beam = doc.GetElement(beamRefer) as FamilyInstance;
                    IList<Reference> elemRefers = uiDoc.Selection.PickObjects(ObjectType.Element, "请选择支座");
                    List<Element> elems = elemRefers.Select(refer => doc.GetElement(refer)).ToList();

                    using (Transaction trans = new Transaction(doc, "拆分梁"))
                    {
                        trans.Start();
                        CutBeamData data = new CutBeamData(doc, beam);
                        data.Update(elems);

                        XYZ[] Points = data.Points;
                        doc.Delete(beam.Id);
                        doc.Regenerate();
                        for (int i = 0; i < Points.Length; i += 2)
                        {
                            if (Points[i].DistanceTo(Points[i + 1]) < 0.01) continue;
                            FamilyInstance newBeam = doc.Create.NewFamilyInstance(Line.CreateBound(Points[i], Points[i + 1]), data.FamilySymbol, data.Level, StructuralType.Beam);
                            newBeam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).Set(data.MaterialId);
                            GlobalUtil.SetSharedParameters(newBeam, data.SharedParameters);
                            IList<Element> inclusion = GlobalUtil.IncludeTest(doc, newBeam);
                            JoinManager.OfBeam(doc, newBeam, inclusion, new JoinOption() { JoinByBeam = true, JoinByWall = true, JoinByColumn = true, JoinByFloor = false });
                        }
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
    }
}
