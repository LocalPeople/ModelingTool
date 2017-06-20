using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            foreach (Parameter param in (from Parameter p in symbol.Parameters where !p.IsReadOnly && p.Definition.ParameterGroup == BuiltInParameterGroup.PG_GEOMETRY select p))
            {
                switch (param.StorageType)
                {
                    case StorageType.Integer:
                        result.Add(param.Definition.Name, param.AsInteger());
                        break;
                    case StorageType.Double:
                        result.Add(param.Definition.Name, param.AsDouble());
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
                        param.Set((int)Math.Round(pair.Value));
                        break;
                    case StorageType.Double:
                        param.Set(pair.Value);
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
                        Document familyDoc = doc.EditFamily(symbol.Family);
                        using (Transaction familyTrans = new Transaction(familyDoc, "删除族类型"))
                        {
                            familyTrans.Start();
                            FamilyManager familyMgr = familyDoc.FamilyManager;
                            familyMgr.CurrentType = familyMgr.Types.Cast<FamilyType>().FirstOrDefault(type => type.Name == symbol.Name);
                            if (familyMgr.CurrentType != null)
                            {
                                familyMgr.DeleteCurrentType();
                                familyDoc.LoadFamily(doc, UIDocument.GetRevitUIFamilyLoadOptions());
                                result = true;
                            }
                            familyTrans.Commit();
                        }
                        break;
                }
            }
            return result;
        }
    }
}
