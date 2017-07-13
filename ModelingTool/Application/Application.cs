using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingTool.Application
{
    class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.GetRibbonPanels().FirstOrDefault(RP => RP.Name == "梁建模");
            if (ribbonPanel == null)
                ribbonPanel = application.CreateRibbonPanel("梁建模");
            ribbonPanel.AddItem(new PushButtonData("QuickCreation", "\n\n布置\n梁", typeof(Application).Assembly.Location, typeof(Beam.QuickCreation.Command).FullName));
            ribbonPanel.AddItem(new PushButtonData("BeamUnion", "\n\n合并\n梁", typeof(Application).Assembly.Location, typeof(Beam.BeamUnion.Command).FullName));
            ribbonPanel.AddItem(new PushButtonData("CutBeam", "\n\n拆分\n梁", typeof(Application).Assembly.Location, typeof(Beam.CutBeam.Command).FullName));
            ribbonPanel.AddItem(new PushButtonData("BeamAlignFloor", "\n\n梁对齐\n斜板", typeof(Application).Assembly.Location, typeof(Beam.BeamAlignFloor.Command).FullName));
            ribbonPanel.AddItem(new PushButtonData("ChangeJoinOrder", "\n\n主梁\n切次梁", typeof(Application).Assembly.Location, typeof(Beam.ChangeJoinOrder.Command).FullName));
            ribbonPanel.AddItem(new PushButtonData("JoinEnd", "\n\n处理\n梁连接", typeof(Application).Assembly.Location, typeof(Beam.JoinEnd.Command).FullName));
            ribbonPanel.AddItem(new PushButtonData("LengthAdjustment", "\n\n批量调整\n梁端点", typeof(Application).Assembly.Location, typeof(Beam.LengthAdjustment.Command).FullName));

            return Result.Succeeded;
        }
    }
}
