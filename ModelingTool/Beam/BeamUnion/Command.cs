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
                    IList<Reference> beamRefers = uiDoc.Selection.PickObjects(ObjectType.Element, BeamSelectionFilter.Single, "请选择需要合并的梁");
                    List<FamilyInstance> beams = beamRefers.Select(refer => doc.GetElement(refer) as FamilyInstance).ToList();

                    using (Transaction trans = new Transaction(doc, "合并梁"))
                    {
                        trans.Start();
                        BeamUnionData data = GetUnionData(doc, beams);
                        if (!data.CanUnion)
                        {
                            throw new InvalidOperationException(data.Message);
                        }
                        Union(doc, beams, data);
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

        private void Union(Document doc, List<FamilyInstance> beams, BeamUnionData data)
        {
            doc.Delete(beams.Select(beam => beam.Id).ToArray());
            doc.Regenerate();

            FamilyInstance newBeam = doc.Create.NewFamilyInstance(Line.CreateBound(data.Start, data.End), data.FamilySymbol, data.Level, StructuralType.Beam);
            newBeam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).Set(data.MaterialId);
            GlobalUtil.SetSharedParameters(newBeam, data.SharedParameters);
            IList<Element> inclusion = GlobalUtil.IncludeTest(doc, newBeam);
            JoinManager.OfBeam(doc, newBeam, inclusion, new JoinOption() { JoinByBeam = false, JoinByWall = true, JoinByColumn = true, JoinByFloor = false });
        }

        private BeamUnionData GetUnionData(Document doc, List<FamilyInstance> beams)
        {
            BeamUnionData data = new BeamUnionData();

            foreach (FamilyInstance beam in beams)
            {
                if (!data.Update(doc, beam))
                {
                    break;
                }
            }

            return data;
        }
    }
}
