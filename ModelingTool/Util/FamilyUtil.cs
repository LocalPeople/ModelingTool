using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ModelingTool.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelingTool.Util
{
    static class FamilyUtil
    {
        public static Dictionary<string, double> GetSymbolGeometry(FamilySymbol symbol)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (Parameter param in (from Parameter p in symbol.Parameters where !p.IsReadOnly && p.Definition.ParameterGroup == BuiltInParameterGroup.PG_GEOMETRY orderby p.Definition.Name select p))
            {
                switch (param.StorageType)
                {
                    case StorageType.Integer:
                        result.Add(param.Definition.Name, param.AsInteger() * 304.8);
                        break;
                    case StorageType.Double:
                        result.Add(param.Definition.Name, param.AsDouble() * 304.8);
                        break;
                }
            }
            return result;
        }

        public static FamilySymbol Duplicate(this FamilySymbol symbol, string name, Dictionary<string, double> geometry)
        {
            FamilySymbol newSymbol = symbol.Duplicate(name) as FamilySymbol;
            foreach (var pair in geometry)
            {
                Parameter param = symbol.LookupParameter(pair.Key);
                switch (param.StorageType)
                {
                    case StorageType.Integer:
                        param.Set((int)(Math.Round(pair.Value / 304.8)));
                        break;
                    case StorageType.Double:
                        param.Set(pair.Value / 304.8);
                        break;
                }
            }
            return newSymbol;
        }

        public static bool Delete(this FamilySymbol symbol, Document doc, IWin32Window owner)
        {
            bool result = false;
            using (FilteredElementCollector collector = new FilteredElementCollector(doc))
            using (FamilyInstanceFilter filter = new FamilyInstanceFilter(doc, symbol.Id))
            {
                int count = collector.WherePasses(filter).ToElements().Count;
                string text = string.Format("将要删除类型“{0}：{1}”及其实例:\n{2} 个实例将被删除.", symbol.FamilyName, symbol.Name, count);
                switch (MessageBox.Show(owner, text, "将要删除类型……", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
                {
                    case DialogResult.OK:
                        doc.Delete(symbol.Id);
                        result = true;
                        break;
                }
            }
            return result;
        }

        public static KeyValuePair<string, FamilySymbol[]> GetFamilySymbolPair(this Family family, Document doc)
        {
            FamilySymbol[] familySymbolSet = family.GetFamilySymbolIds().Select(id => doc.GetElement(id) as FamilySymbol).ToArray();
            Array.Sort(familySymbolSet, SymbolNameComparer.Single);
            return new KeyValuePair<string, FamilySymbol[]>(family.Name, familySymbolSet);
        }
    }
}
