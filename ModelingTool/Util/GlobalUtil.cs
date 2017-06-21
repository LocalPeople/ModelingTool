using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Util
{
    static class GlobalUtil
    {
        public static IList<Element> IncludeTest(Document doc, Element elem)
        {
            doc.Regenerate();
            Solid solid = SolidSearch(elem.get_Geometry(new Options()));
            FilteredElementCollector collector = new FilteredElementCollector(doc).Excluding(new ElementId[] { elem.Id });
            ElementIntersectsSolidFilter solidFilter = new ElementIntersectsSolidFilter(solid);
            return collector.WherePasses(solidFilter).ToElements();
        }

        public static Solid SolidSearch(GeometryElement geomElem)
        {
            Solid result = null;
            foreach (GeometryObject geomObj in geomElem)
            {
                Solid solid = geomObj as Solid;
                if (null != solid && solid.Volume != 0)
                {
                    result = solid;
                    break;
                }
                GeometryInstance geomInst = geomObj as GeometryInstance;
                if (null != geomInst)
                {
                    GeometryElement transformedGeomElem = geomInst.GetInstanceGeometry();
                    result = SolidSearch(transformedGeomElem);
                    if (result != null)
                        break;
                }
            }
            return result;
        }

        public static void GetBeamDimension(Element beam, out double height, out double width)
        {
            height = 0.0;
            width = 0.0;
            if (!(beam is FamilyInstance)) return;
            XYZ beamDirection = ((beam.Location as LocationCurve).Curve as Line).Direction;
            Solid beamSolid = SolidSearch(beam.get_Geometry(new Options()));
            double volumn = double.MinValue;
            foreach (Face face in beamSolid.Faces)
            {
                XYZ faceDirection = face.ComputeNormal(new UV());
                if (faceDirection.IsAlmostEqualTo(beamDirection, false) || faceDirection.IsAlmostEqualTo(-beamDirection, false))
                {
                    if (volumn < face.Area)
                    {
                        BoundingBoxUV uvBox = face.GetBoundingBox();
                        XYZ min = face.Evaluate(uvBox.Min);
                        XYZ max = face.Evaluate(uvBox.Max);
                        // 勾股定理
                        height = Math.Abs(max.Z - min.Z);
                        double third = max.DistanceTo(min);
                        width = Math.Sqrt(third * third - height * height);
                        volumn = face.Area;
                    }
                }
            }
        }

        public static Dictionary<Guid, object> GetSharedParameters(Document doc, Element elem)
        {
            Dictionary<Guid, object> result = new Dictionary<Guid, object>();
            foreach (Parameter param in (from Parameter param in elem.Parameters where !param.IsReadOnly && param.IsShared select param))
            {
                switch (param.StorageType)
                {
                    case StorageType.Integer:
                        result.Add(param.GUID, param.AsInteger());
                        break;
                    case StorageType.Double:
                        result.Add(param.GUID, param.AsDouble());
                        break;
                    case StorageType.String:
                        result.Add(param.GUID, param.AsString());
                        break;
                    case StorageType.ElementId:
                        result.Add(param.GUID, param.AsElementId());
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public static void SetSharedParameters(Element elem, Dictionary<Guid, object> sharedParameters)
        {
            foreach (var shared in sharedParameters)
            {
                SetParamter(elem, shared.Key, shared.Value);
            }
        }

        private static void SetParamter(Element elem, Guid guid, object value)
        {
            Parameter param = elem.get_Parameter(guid);
            if (param != null && !param.IsReadOnly)
            {
                switch (param.StorageType)
                {
                    case StorageType.Integer:
                        param.Set((int)value);
                        break;
                    case StorageType.Double:
                        param.Set((double)value);
                        break;
                    case StorageType.String:
                        param.Set((string)value);
                        break;
                    case StorageType.ElementId:
                        param.Set((ElementId)value);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 修复XYZ.IsAlmostEqualTo误差值不准确的拓展方法
        /// </summary>
        public static bool IsAlmostEqualTo(this XYZ self, XYZ other, bool ignoreZ)
        {
            return Math.Abs(self.X - other.X) < 0.01
                && Math.Abs(self.Y - other.Y) < 0.01
                && (ignoreZ || Math.Abs(self.Z - other.Z) < 0.01);
        }
    }
}
