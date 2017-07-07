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
        public static void OfBeam(Document doc, FamilyInstance beam, IList<Element> inclusion, JoinOption option)
        {
            foreach (Element elem in inclusion)
            {
                if (IsBeam(elem))
                {
                    if (option.JoinByBeam)
                        Join(doc, elem, beam);
                    else
                        Join(doc, beam, elem);
                }
                else if (IsWall(elem))
                {
                    if (option.JoinByWall)
                        Join(doc, elem, beam);
                    else
                        Join(doc, beam, elem);
                }
                else if (IsFloor(elem))
                {
                    if (option.JoinByFloor)
                        Join(doc, elem, beam);
                    else
                        Join(doc, beam, elem);
                }
                else if (IsColumn(elem))
                {
                    if (option.JoinByColumn)
                        Join(doc, elem, beam);
                    else
                        Join(doc, beam, elem);
                }
            }
        }

        public static void OfWall(Document doc, Wall wall, List<Element> exclusion)
        {

        }

        public static void OfColumn(Document doc, FamilyInstance column, List<Element> exclusion)
        {

        }

        public static void OfFloor(Document doc, Floor floor, List<Element> exclusion)
        {

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
