using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using ModelingTool.Filter.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.CutBeam
{
    class CutOperationMap
    {
        delegate XYZ CutPointHandler(Line CutBeamLine, Element elem);

        Dictionary<ISelectionFilter, CutPointHandler> map;

        public CutOperationMap()
        {
            map = new Dictionary<ISelectionFilter, CutPointHandler>();
            map.Add(BeamSelectionFilter.Single, new BeamPointGetter().GetPoint);
            map.Add(WallSelectionFilter.Single, new WallPointGetter().GetPoint);
            map.Add(ColumnSelectionFilter.Single, new ColumnPointGetter().GetPoint);
        }

        public XYZ GetPoint(Line CutBeamLine, Element elem)
        {
            foreach (var operation in map)
            {
                if (operation.Key.AllowElement(elem))
                {
                    return operation.Value(CutBeamLine, elem);
                }
            }
            return null;
        }
    }
}
