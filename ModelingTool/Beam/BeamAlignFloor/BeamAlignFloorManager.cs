using Autodesk.Revit.DB;
using ModelingTool.Interface;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Beam.BeamAlignFloor
{
    class BeamAlignFloorManager
    {
        bool alignTop;
        List<Curve> floorFaceEdges;
        Level beamLevel;
        Line beamLine;
        Line horizontalBeamLine;
        List<XYZ> drawPoints;

        public FamilySymbol FamilySymbol { get; set; }
        public Level Level { get; set; }
        public ElementId MaterialId { get; set; }
        public List<XYZ> Points { get { return drawPoints; } }

        public void SetAlignTop(bool alignTop)
        {
            this.alignTop = alignTop;
        }

        public void SetFloor(Floor floor)
        {
            Solid floorSolid = GlobalUtil.SolidSearch(floor.get_Geometry(new Options()));
            PlanarFace maxVolumnFace = null;
            int reverse = alignTop ? 1 : -1;
            foreach (Face face in floorSolid.Faces)
            {
                PlanarFace planarFace = face as PlanarFace;
                if (planarFace == null)
                {
                    continue;
                }
                if (reverse * planarFace.ComputeNormal(new UV()).Z < 0)
                {
                    continue;
                }
                if (maxVolumnFace == null)
                {
                    maxVolumnFace = planarFace;
                }
                if (face.Area > maxVolumnFace.Area)
                {
                    maxVolumnFace = planarFace;
                }
            }
            floorFaceEdges = new List<Curve>();
            foreach (EdgeArray edgeArray in maxVolumnFace.EdgeLoops)
            {
                foreach (Edge edge in edgeArray)
                {
                    floorFaceEdges.Add(edge.AsCurve());
                }
            }
        }

        public void SetBeam(Document doc, FamilyInstance beam)
        {
            beamLine = beam.ToLine(true);
            beamLevel = doc.GetElement(beam.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).AsElementId()) as Level;
            XYZ beamStart = beamLine.GetEndPoint(0);
            XYZ beamEnd = beamLine.GetEndPoint(1);
            beamStart = new XYZ(beamStart.X, beamStart.Y, beamLevel.Elevation);
            beamEnd = new XYZ(beamEnd.X, beamEnd.Y, beamLevel.Elevation);

            /// 这段投影并延长产生相交运算小数位精度误差
            XYZ direction = (beamEnd - beamStart).Normalize();
            horizontalBeamLine = Line.CreateBound(beamStart - direction * 1000, beamEnd + direction * 1000);

            FamilySymbol = beam.Symbol;
            Level = doc.GetElement(beam.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).AsElementId()) as Level;
            MaterialId = beam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId();
        }

        public void Init()
        {
            List<XYZ> boundaryPoints = new List<XYZ>();
            foreach (Curve curve in floorFaceEdges)
            {
                Curve horizontalCurve;
                if (curve is Line)
                {
                    Line line = curve as Line;
                    XYZ lineStart = line.GetEndPoint(0);
                    XYZ lineEnd = line.GetEndPoint(1);
                    horizontalCurve = Line.CreateBound(new XYZ(lineStart.X, lineStart.Y, beamLevel.Elevation), new XYZ(lineEnd.X, lineEnd.Y, beamLevel.Elevation));
                }
                else if (curve is Arc)
                {
                    Arc arc = curve as Arc;
                    XYZ arcStart = arc.GetEndPoint(0);
                    XYZ arcEnd = arc.GetEndPoint(1);
                    XYZ normalStart = (arcStart - arc.Center).Normalize();
                    XYZ normalEnd = (arcEnd - arc.Center).Normalize();
                    XYZ pointOnArc = Transform.CreateRotation(arc.Normal, normalStart.AngleTo(normalEnd) * normalStart.CrossProduct(normalEnd).Z / 2).OfPoint(arcStart);
                    horizontalCurve = Arc.Create(new XYZ(arcStart.X, arcStart.Y, beamLevel.Elevation), new XYZ(arcEnd.X, arcEnd.Y, beamLevel.Elevation), new XYZ(pointOnArc.X, pointOnArc.Y, beamLevel.Elevation));
                }
                else
                {
                    HermiteSpline spline = curve as HermiteSpline;
                    horizontalCurve = HermiteSpline.Create(spline.ControlPoints.Select(pt => new XYZ(pt.X, pt.Y, beamLevel.Elevation)).ToList(), false);
                }
                IntersectionResultArray resultArray1;
                if (SetComparisonResult.Overlap == horizontalCurve.Intersect(horizontalBeamLine, out resultArray1))
                {
                    for (int i = 0; i < resultArray1.Size; i++)
                    {
                        XYZ aPoint = resultArray1.get_Item(i).XYZPoint;
                        Line verticalLine = Line.CreateBound(aPoint - XYZ.BasisZ * 1000, aPoint + XYZ.BasisZ * 1000);
                        IntersectionResultArray resultArray2;
                        verticalLine.Intersect(curve, out resultArray2);
                        XYZ bPoint = resultArray2.get_Item(0).XYZPoint;
                        boundaryPoints.Add(new XYZ(Math.Round(bPoint.X, 4), Math.Round(bPoint.Y, 4), Math.Round(bPoint.Z, 4)));
                    }
                }
            }

            drawPoints = new List<XYZ>();
            AxisPointsComparer pointComparer = new AxisPointsComparer(horizontalBeamLine.Direction);
            boundaryPoints.Sort(pointComparer);
            XYZ start = beamLine.GetEndPoint(0);
            XYZ end = beamLine.GetEndPoint(1);
            drawPoints.Add(GetRightEndPoint(start, boundaryPoints));
            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                if (pointComparer.Compare(start, boundaryPoints[i]) <= 0 && pointComparer.Compare(end, boundaryPoints[i]) >= 0)
                {
                    IntersectionResultArray resultArray;
                    Line.CreateBound(boundaryPoints[i] - XYZ.BasisZ * 1000, boundaryPoints[i] + XYZ.BasisZ * 1000).Intersect(beamLine, out resultArray);
                    switch (i % 2)
                    {
                        case 0:
                            drawPoints.Add(resultArray.get_Item(0).XYZPoint);
                            drawPoints.Add(boundaryPoints[i]);
                            break;
                        case 1:
                            drawPoints.Add(boundaryPoints[i]);
                            drawPoints.Add(resultArray.get_Item(0).XYZPoint);
                            break;
                    }
                }
            }
            drawPoints.Add(GetRightEndPoint(end, boundaryPoints));
        }

        private XYZ GetRightEndPoint(XYZ endPoint, List<XYZ> boundaryPoints)
        {
            Line verticalLine = Line.CreateBound(endPoint - XYZ.BasisZ * 1000, endPoint + XYZ.BasisZ * 1000);
            for (int i = 0; i < boundaryPoints.Count; i += 2)
            {
                Line floorLine = Line.CreateBound(boundaryPoints[i], boundaryPoints[i + 1]);
                IntersectionResultArray resultArray;
                if (SetComparisonResult.Overlap == floorLine.Intersect(verticalLine, out resultArray))
                {
                    return resultArray.get_Item(0).XYZPoint;
                }
            }
            return endPoint;
        }
    }
}
