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
            this.axis = new XYZ(axis.X, axis.Y, 0);
        }

        public int Compare(XYZ x, XYZ y)
        {
            XYZ xDirection = new XYZ(x.X, x.Y, 0).Normalize();
            XYZ yDirection = new XYZ(y.X, y.Y, 0).Normalize();
            return xDirection.DotProduct(axis).CompareTo(yDirection.DotProduct(axis));
        }
    }
}
