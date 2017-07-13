using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Util
{
    static class JoinManager
    {
        public static void CollectorJoin(Document doc, Element element, IList<Element> inclusion, JoinOption option)
        {
            foreach (Element other in inclusion)
            {
                if (IsBeam(other))
                {
                    if (option.JoinByBeam)
                        Join(doc, other, element);
                    else
                        Join(doc, element, other);
                }
                else if (IsWall(other))
                {
                    if (option.JoinByWall)
                        Join(doc, other, element);
                    else
                        Join(doc, element, other);
                }
                else if (IsFloor(other))
                {
                    if (option.JoinByFloor)
                        Join(doc, other, element);
                    else
                        Join(doc, element, other);
                }
                else if (IsColumn(other))
                {
                    if (option.JoinByColumn)
                        Join(doc, other, element);
                    else
                        Join(doc, element, other);
                }
            }
        }

        public static void Join(Document doc, Element first, Element second)
        {
            if (!JoinGeometryUtils.AreElementsJoined(doc, first, second))
            {
                JoinGeometryUtils.JoinGeometry(doc, first, second);
                if (JoinGeometryUtils.IsCuttingElementInJoin(doc, second, first))
                {
                    JoinGeometryUtils.SwitchJoinOrder(doc, first, second);
                }
            }
            else
            {
                if (JoinGeometryUtils.IsCuttingElementInJoin(doc, second, first))
                {
                    JoinGeometryUtils.SwitchJoinOrder(doc, first, second);
                }
            }
        }

        public static void ClearJoin(Document doc, Element elem)
        {
            foreach (ElementId other in JoinGeometryUtils.GetJoinedElements(doc, elem))
            {
                JoinGeometryUtils.UnjoinGeometry(doc, elem, doc.GetElement(other));
            }
        }

        private static bool IsBeam(Element elem)
        {
            return elem.Category.Id == new ElementId((int)BuiltInCategory.OST_StructuralFraming);
        }

        private static bool IsWall(Element elem)
        {
            return elem.Category.Id == new ElementId((int)BuiltInCategory.OST_Walls);
        }

        private static bool IsFloor(Element elem)
        {
            return elem.Category.Id == new ElementId((int)BuiltInCategory.OST_Floors);
        }

        private static bool IsColumn(Element elem)
        {
            return elem.Category.Id == new ElementId((int)BuiltInCategory.OST_StructuralColumns);
        }
    }
}
