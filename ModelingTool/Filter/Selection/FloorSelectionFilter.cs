using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Filter.Selection
{
    class FloorSelectionFilter : ISelectionFilter
    {
        public static FloorSelectionFilter Single = new FloorSelectionFilter();

        public bool AllowElement(Element elem)
        {
            return elem is Floor &&
                elem.Category.Id == new ElementId((int)BuiltInCategory.OST_Floors);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
