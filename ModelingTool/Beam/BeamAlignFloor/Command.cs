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

namespace ModelingTool.Beam.BeamAlignFloor
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            BeamAlignFloorForm form = new BeamAlignFloorForm();
            if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    Reference floorRefer = uiDoc.Selection.PickObject(ObjectType.Element, FloorSelectionFilter.Single, "请选择需要对齐的板");
                    Floor floor = doc.GetElement(floorRefer) as Floor;
                    IList<Reference> beamRefers = uiDoc.Selection.PickObjects(ObjectType.Element, BeamSelectionFilter.Single, "请选择需要对齐的梁");
                    FamilyInstance[] beams = beamRefers.Select(refer => doc.GetElement(refer)).Cast<FamilyInstance>().ToArray();

                    using (Transaction trans = new Transaction(doc, "梁齐板"))
                    {
                        trans.Start();
                        BeamAlignFloorManager manager = new BeamAlignFloorManager();
                        manager.SetAlignTop(form.AlignTop);
                        manager.SetFloor(floor);

                        foreach (FamilyInstance beam in beams)
                        {
                            manager.SetBeam(doc, beam);
                            manager.Init();

                            doc.Delete(beam.Id);
                            doc.Regenerate();
                            for (int i = 0; i < manager.Points.Count; i += 2)
                            {
                                if (manager.Points[i].DistanceTo(manager.Points[i + 1]) < 0.01)
                                    continue;
                                FamilyInstance newBeam = doc.Create.NewFamilyInstance(Line.CreateBound(manager.Points[i], manager.Points[i + 1]), manager.FamilySymbol, manager.Level, StructuralType.Beam);
                                newBeam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).Set(manager.MaterialId);
                            }
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