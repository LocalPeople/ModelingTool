using Autodesk.Revit.DB;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.CutBeam
{
    interface IPointGetter
    {
        XYZ GetPoint(Line CutBeamLine, Element elem);
    }

    class BeamPointGetter : IPointGetter
    {
        public XYZ GetPoint(Line CutBeamLine, Element elem)
        {
            FamilyInstance beam = elem as FamilyInstance;
            Line real = GlobalUtil.GetBeamCenterLine(beam);
            double Z = CutBeamLine.GetEndPoint(0).Z;
            real = Line.CreateBound(new XYZ(real.GetEndPoint(0).X, real.GetEndPoint(0).Y, Z), new XYZ(real.GetEndPoint(1).X, real.GetEndPoint(1).Y, Z));
            IntersectionResultArray resultArray;
            if (SetComparisonResult.Overlap == real.Intersect(CutBeamLine, out resultArray))
            {
                return resultArray.get_Item(0).XYZPoint;
            }
            return null;
        }
    }

    class WallPointGetter : IPointGetter
    {
        public XYZ GetPoint(Line CutBeamLine, Element elem)
        {
            Wall wall = elem as Wall;
            Line real = wall.ToLine();
            double Z = CutBeamLine.GetEndPoint(0).Z;
            real = Line.CreateBound(new XYZ(real.GetEndPoint(0).X, real.GetEndPoint(0).Y, Z), new XYZ(real.GetEndPoint(1).X, real.GetEndPoint(1).Y, Z));
            IntersectionResultArray resultArray;
            if (SetComparisonResult.Overlap == real.Intersect(CutBeamLine, out resultArray))
            {
                return resultArray.get_Item(0).XYZPoint;
            }
            return null;
        }
    }

    class ColumnPointGetter : IPointGetter
    {
        public XYZ GetPoint(Line CutBeamLine, Element elem)
        {
            FamilyInstance column = elem as FamilyInstance;
            XYZ location = column.ToPoint();
            return GlobalUtil.IsPointOverLine(CutBeamLine, location) ? CutBeamLine.Project(location).XYZPoint : null;
        }
    }
}
