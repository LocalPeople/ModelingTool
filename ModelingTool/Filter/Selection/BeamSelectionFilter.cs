using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ModelingTool.Filter.Selection
{
    class BeamSelectionFilter : ISelectionFilter
    {
        public static BeamSelectionFilter Single = new BeamSelectionFilter();

        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance &&
                elem.Category.Id == new ElementId((int)BuiltInCategory.OST_StructuralFraming);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
