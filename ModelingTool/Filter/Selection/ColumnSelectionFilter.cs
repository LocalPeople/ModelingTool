using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ModelingTool.Filter.Selection
{
    class ColumnSelectionFilter : ISelectionFilter
    {
        public static ColumnSelectionFilter Single = new ColumnSelectionFilter();

        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance &&
                elem.Category.Id == new ElementId((int)BuiltInCategory.OST_StructuralColumns);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
