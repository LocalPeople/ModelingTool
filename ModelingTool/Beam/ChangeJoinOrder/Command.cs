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

namespace ModelingTool.Beam.ChangeJoinOrder
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            IList<Reference> refers = uiDoc.Selection.PickObjects(ObjectType.Element, BeamSelectionFilter.Single, "请选择需要处理的梁");
            Queue<ElementId> idsQueue = new Queue<ElementId>(refers.Select(refer => refer.ElementId));

            using (Transaction trans = new Transaction(doc, "主梁切次梁"))
            {
                trans.Start();
                CheckChangeJoinOrder(doc, idsQueue);
                trans.Commit();
            }

            return Result.Succeeded;
        }

        private void CheckChangeJoinOrder(Document doc, Queue<ElementId> idsQueue)
        {
            double height1, width1, height2, width2;
            while (idsQueue.Count > 1)
            {
                Element beam = doc.GetElement(idsQueue.Dequeue());
                GlobalUtil.GetBeamDimension(beam, out height1, out width1);
                using (ElementIntersectsSolidFilter solidFilter = new ElementIntersectsSolidFilter(GlobalUtil.SolidSearch(beam.get_Geometry(new Options()))))
                using (FilteredElementCollector collector = new FilteredElementCollector(doc, idsQueue.ToArray()))
                {
                    foreach (ElementId id in JoinGeometryUtils.GetJoinedElements(doc, beam))
                    {
                        Element other = doc.GetElement(id);
                        if (IsInQueue(other, idsQueue))
                        {
                            collector.Excluding(new ElementId[] { id });
                            GlobalUtil.GetBeamDimension(other, out height2, out width2);
                            if (height1 * width1 > height2 * width2)
                            {
                                JoinManager.Join(doc, beam, other);
                            }
                            else
                            {
                                JoinManager.Join(doc, other, beam);
                            }
                        }
                    }
                    foreach (Element other in collector.WherePasses(solidFilter))
                    {
                        if (IsInQueue(other, idsQueue))
                        {
                            GlobalUtil.GetBeamDimension(other, out height2, out width2);
                            if (height1 * width1 > height2 * width2)
                            {
                                JoinManager.Join(doc, beam, other);
                            }
                            else
                            {
                                JoinManager.Join(doc, other, beam);
                            }
                        }
                    }
                }
            }
        }

        private bool IsInQueue(Element elem, Queue<ElementId> queue)
        {
            return queue.Contains(elem.Id);
        }
    }
}
