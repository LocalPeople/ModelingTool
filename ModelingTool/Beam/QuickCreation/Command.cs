using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ModelingTool.Filter.Document;
using ModelingTool.Filter.Selection;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.QuickCreation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            FilteredElementCollector levelCollector = new FilteredElementCollector(doc).OfClass(typeof(Level));
            FilteredElementCollector beamFamilySymbolCollector = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming);
            QuickCreationForm form = new QuickCreationForm(doc);
            form.InitFamilySymbolMap(beamFamilySymbolCollector.GetFamilySymbolMap(doc));
            form.InitLevelList(levelCollector.ToElements());
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    if (form.ByGrid)
                    {
                        Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, GridSelectionFilter.Single, "请选择单根轴线");
                        Grid grid = doc.GetElement(refer) as Grid;

                        using (Transaction trans = new Transaction(doc, "布置梁"))
                        {
                            trans.Start();
                            CreateBeamByGrid(doc, grid, form.SelectedLevel, form.SelectedFamilySymbol);
                            trans.Commit();
                        }
                    }
                    else if (form.ByPoint)
                    {
                        XYZ[] endPoints = new XYZ[2];
                        endPoints[0] = uiDoc.Selection.PickPoint(ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections | ObjectSnapTypes.Midpoints, "请点选梁起点");
                        endPoints[1] = uiDoc.Selection.PickPoint(ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections | ObjectSnapTypes.Midpoints, "请点选梁终点");

                        using (Transaction trans = new Transaction(doc, "布置梁"))
                        {
                            trans.Start();
                            CreateBeamByTwoPoint(doc, endPoints, form.SelectedLevel, form.SelectedFamilySymbol);
                            trans.Commit();
                        }
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                    {
                        break;
                    }
                }
                catch
                {
                    throw;
                }
            }

            return Result.Succeeded;
        }

        private void CreateBeamByGrid(Document doc, Grid grid, Level levelAt, FamilySymbol beamFamilySymbol)
        {
            if (!beamFamilySymbol.IsActive)
                beamFamilySymbol.Activate();
            FamilyInstance beam = doc.Create.NewFamilyInstance(grid.Curve, beamFamilySymbol, levelAt, Autodesk.Revit.DB.Structure.StructuralType.Beam);
            beam.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).Set(levelAt.Id);
            beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).Set(0.0);
            beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION).Set(0.0);
            IList<Element> inclusion = GlobalUtil.IncludeTest(doc, beam);
            JoinManager.OfBeam(doc, beam, inclusion);
        }

        private void CreateBeamByTwoPoint(Document doc, XYZ[] endPoints, Level levelAt, FamilySymbol beamFamilySymbol)
        {
            if (!beamFamilySymbol.IsActive)
                beamFamilySymbol.Activate();
            FamilyInstance beam = doc.Create.NewFamilyInstance(Line.CreateBound(endPoints[0], endPoints[1]), beamFamilySymbol, levelAt, Autodesk.Revit.DB.Structure.StructuralType.Beam);
            IList<Element> inclusion = GlobalUtil.IncludeTest(doc, beam);
            JoinManager.OfBeam(doc, beam, inclusion);
        }
    }
}
