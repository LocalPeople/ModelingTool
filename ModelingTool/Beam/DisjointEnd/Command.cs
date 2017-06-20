using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ModelingTool.Filter.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelingTool.Beam.DisjointEnd
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            DisjointEndForm form = new DisjointEndForm();
            if (form.ShowDialog() == DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    using (Transaction trans = new Transaction(doc, "不允许梁连接"))
                    {
                        trans.Start();
                        if (form.OnePointDisjoint)
                        {
                            Reference refer = uiDoc.Selection.PickObject(ObjectType.PointOnElement, "点选梁端点");
                            FamilyInstance beam = doc.GetElement(refer) as FamilyInstance;
                            if (beam != null)
                            {
                                LocationCurve locationCurve = beam.Location as LocationCurve;
                                if (refer.GlobalPoint.DistanceTo(locationCurve.Curve.GetEndPoint(0)) < refer.GlobalPoint.DistanceTo(locationCurve.Curve.GetEndPoint(1)))
                                {
                                    DisJointBeam(beam, DisjointAction.Start);
                                }
                                else
                                {
                                    DisJointBeam(beam, DisjointAction.End);
                                }
                            }
                        }
                        else if (form.OneBeamDisjoint)
                        {
                            Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, BeamSelectionFilter.Single, "选单根梁");
                            FamilyInstance beam = doc.GetElement(refer) as FamilyInstance;
                            if (beam != null)
                            {
                                DisJointBeam(beam, DisjointAction.Both);
                            }
                        }
                        else if (form.MultiBeamDisjoint)
                        {
                            IList<Reference> refers = uiDoc.Selection.PickObjects(ObjectType.Element, BeamSelectionFilter.Single, "框选梁");
                            foreach (Reference refer in refers)
                            {
                                FamilyInstance beam = doc.GetElement(refer) as FamilyInstance;
                                if (beam != null)
                                {
                                    DisJointBeam(beam, DisjointAction.Both);
                                }
                            }
                        }
                        trans.Commit();
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    throw;
                }
            }

            return Result.Succeeded;
        }

        private void DisJointBeam(FamilyInstance beam, DisjointAction action)
        {
            switch (action)
            {
                case DisjointAction.Start:
                    StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
                    break;
                case DisjointAction.End:
                    StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);
                    break;
                case DisjointAction.Both:
                    StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
                    StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);
                    break;
            }
        }
    }
}
