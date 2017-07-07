using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ModelingTool.Filter.Selection
{
    class WallSelectionFilter : ISelectionFilter
    {
        public static WallSelectionFilter Single = new WallSelectionFilter();

        public bool AllowElement(Element elem)
        {
            return elem is Wall &&
                elem.Category.Id == new ElementId((int)BuiltInCategory.OST_Walls);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
