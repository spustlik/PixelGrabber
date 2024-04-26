using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    public interface ILayoutManagerService
    {
        void LoadLayout(XElement parent);
        void SaveLayout(XElement parent);
    }

    partial class ShellWindowContent : ILayoutManagerService
    {
        private const string ATTR_TYPE = "Type";
        private const string ELE_LAYOUT = "Layout";
        void ILayoutManagerService.LoadLayout(XElement parent)
        {
            foreach (var ele in parent.Elements(nameof(ViewPart)))
            {
                LoadViewPart(ele);
            }
        }

        private void LoadViewPart(XElement ele)
        {
            var typeId = ele.Attribute(ATTR_TYPE)?.Value;
            var def = viewPartFactory.FindTypeDefinition(typeId);
            if (def != null)
            {
                var viewPart = def.Create();
                viewPart.OnLoadLayout(ele);
            }
        }

        void ILayoutManagerService.SaveLayout(XElement parent)
        {
            //todo: file name, shellVm

            foreach (var viewPart in ViewParts)
            {
                SaveViewPart(parent, viewPart);
            }
        }

        private void SaveViewPart(XElement parent, ViewPart viewPart)
        {
            var def = viewPartFactory.FindDefinition(viewPart);
            var ele = parent.Elements(nameof(ViewPart)).FirstOrDefault(e => e.Attribute(ATTR_TYPE)?.Value == def.TypeId);
            if (ele != null)
                ele.Remove();
            var (vpc,index) = GetViewPartControl(viewPart);
            ele = new XElement(nameof(ViewPart),
                new XAttribute(ATTR_TYPE, def.TypeId),
                new XAttribute(nameof(Width), vpc.ActualWidth)
                );
            parent.Add(ele);
            var le = new XElement(ELE_LAYOUT);
            viewPart.OnSaveLayout(le);
            ele.Add(le);
        }

    }
}
