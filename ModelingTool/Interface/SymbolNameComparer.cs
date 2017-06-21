using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModelingTool.Interface
{
    class SymbolNameComparer : IComparer<FamilySymbol>
    {
        public static SymbolNameComparer Single = new SymbolNameComparer();
        private Regex regexArea;

        public SymbolNameComparer()
        {
            regexArea = new Regex("([1-9]\\d*)\\s*[×xX*]\\s*([1-9]\\d*)");
        }

        public int Compare(FamilySymbol x, FamilySymbol y)
        {
            Match xMatch = regexArea.Match(x.Name);
            Match yMatch = regexArea.Match(y.Name);
            if (xMatch.Groups[0].Length == 0 && yMatch.Groups[0].Length == 0)
                return x.Name.CompareTo(y.Name);
            if (xMatch.Groups[0].Length == 0)
                return 1;
            if (yMatch.Groups[0].Length == 0)
                return -1;
            double xArea = double.Parse(xMatch.Groups[1].Value) * double.Parse(xMatch.Groups[2].Value);
            double yArea = double.Parse(yMatch.Groups[1].Value) * double.Parse(yMatch.Groups[2].Value);
            return xArea.CompareTo(yArea);
        }
    }
}
