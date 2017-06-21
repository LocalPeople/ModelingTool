using Autodesk.Revit.DB;
using ModelingTool.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Filter.Document
{
    static class FilteredElementCollectorEx
    {
        public static TResult OfName<TResult>(this FilteredElementCollector collector, string name) where TResult : Element
        {
            return collector.FirstOrDefault(elem => elem.Name == name) as TResult;
        }

        public static Dictionary<string, FamilySymbol[]> GetFamilySymbolMap(this FilteredElementCollector collector, Autodesk.Revit.DB.Document doc)
        {
            if (collector.FirstOrDefault(elem => elem is FamilySymbol) == null)
                throw new InvalidOperationException("集合中不存在任何Autodesk.Revit.DB.FamilySymbol对象");
            Dictionary<string, FamilySymbol[]> result = new Dictionary<string, FamilySymbol[]>();
            foreach (Element elem in collector)
            {
                if (elem is FamilySymbol)
                {
                    FamilySymbol familySymbol = (FamilySymbol)elem;
                    if (!result.ContainsKey(familySymbol.Family.Name))
                    {
                        FamilySymbol[] familySymbolSet = familySymbol.Family.GetFamilySymbolIds().Select(id => doc.GetElement(id) as FamilySymbol).ToArray();
                        Array.Sort(familySymbolSet, SymbolNameComparer.Single);
                        result.Add(familySymbol.Family.Name, familySymbolSet);
                    }
                }
            }
            return result;
        }
    }
}
