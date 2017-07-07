using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ModelingTool.Filter.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelingTool.Beam.JoinEnd
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            JoinEndForm form = new JoinEndForm();
            if (form.ShowDialog() == DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    using (Transaction trans = new Transaction(doc, "处理梁连接"))
                    {
                        trans.Start();
                        if (form.OnePointJoin)
                        {
                            Reference refer = uiDoc.Selection.PickObject(ObjectType.PointOnElement, "点选梁端点");
                            FamilyInstance beam = doc.GetElement(refer) as FamilyInstance;
                            if (beam != null)
                            {
                                LocationCurve locationCurve = beam.Location as LocationCurve;
                                if (refer.GlobalPoint.DistanceTo(locationCurve.Curve.GetEndPoint(0)) < refer.GlobalPoint.DistanceTo(locationCurve.Curve.GetEndPoint(1)))
                                {
                                    JoinBeam(beam, JoinAction.Start);
                                }
                                else
                                {
                                    JoinBeam(beam, JoinAction.End);
                                }
                            }
                        }
                        else if (form.OneBeamJoin)
                        {
                            Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, BeamSelectionFilter.Single, "选单根梁");
                            FamilyInstance beam = doc.GetElement(refer) as FamilyInstance;
                            if (beam != null)
                            {
                                JoinBeam(beam, JoinAction.Both);
                            }
                        }
                        else if (form.MultiBeamJoin)
                        {
                            IList<Reference> refers = uiDoc.Selection.PickObjects(ObjectType.Element, BeamSelectionFilter.Single, "框选梁");
                            foreach (Reference refer in refers)
                            {
                                FamilyInstance beam = doc.GetElement(refer) as FamilyInstance;
                                if (beam != null)
                                {
                                    JoinBeam(beam, JoinAction.Both);
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
            }

            return Result.Succeeded;
        }

        private void JoinBeam(FamilyInstance beam, JoinAction action)
        {
            switch (action)
            {
                case JoinAction.Start:
                    if (StructuralFramingUtils.IsJoinAllowedAtEnd(beam, 0))
                    {
                        StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
                    }
                    else
                    {
                        StructuralFramingUtils.AllowJoinAtEnd(beam, 0);
                    }
                    break;
                case JoinAction.End:
                    if (StructuralFramingUtils.IsJoinAllowedAtEnd(beam, 1))
                    {
                        StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);
                    }
                    else
                    {
                        StructuralFramingUtils.AllowJoinAtEnd(beam, 1);
                    }
                    break;
                case JoinAction.Both:
                    switch (beam.get_Parameter(BuiltInParameter.STRUCT_FRAM_JOIN_STATUS).AsInteger())
                    {
                        case 0:
                            StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
                            StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);
                            break;
                        case 1:
                            StructuralFramingUtils.AllowJoinAtEnd(beam, 0);
                            StructuralFramingUtils.AllowJoinAtEnd(beam, 1);
                            break;
                    }
                    break;
            }
        }
    }
}
