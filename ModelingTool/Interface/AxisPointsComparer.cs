using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Interface
{
    class AxisPointsComparer : IComparer<XYZ>
    {
        XYZ axis;

        public AxisPointsComparer(XYZ axis)
        {
            this.axis = axis;
        }

        public int Compare(XYZ x, XYZ y)
        {
            XYZ xDirection = x.Normalize();
            XYZ yDirection = y.Normalize();
            return xDirection.DotProduct(axis).CompareTo(yDirection.DotProduct(axis));
        }
    }
}
