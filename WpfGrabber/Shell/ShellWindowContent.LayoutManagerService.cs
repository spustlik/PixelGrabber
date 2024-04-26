using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        private const string ELE_FILE = "File";
        private const string ATTR_FILENAME = "FileName";
        private const string ATTR_ZOOM = "Zoom";
        private const string ATTR_OFFSET = "Offset";
        private const string ELE_VIEWPART = "View";
        private const string ATTR_TYPE = "Type";
        private const string ELE_LAYOUT = "Layout";

        private string GetFileName()
        {
            return Path.GetFileName(shellVm.FileName).ToUpperInvariant();
        }

        void ILayoutManagerService.LoadLayout(XElement parent)
        {
            var fileName = GetFileName();
            var ele = parent.Elements(ELE_FILE)
                .FirstOrDefault(x => x.Attribute(ATTR_FILENAME)?.Value == fileName);
            if (ele == null)
                return;
            shellVm.Zoom = (double?)ele.Attribute(ATTR_ZOOM) ?? shellVm.Zoom;
            shellVm.Offset = (int?)ele.Attribute(ATTR_OFFSET) ?? 0;
            foreach (var e in ele.Elements(ELE_VIEWPART))
            {
                LoadViewPart(e);
            }
        }

        private void LoadViewPart(XElement ele)
        {
            var typeId = ele.Attribute(ATTR_TYPE)?.Value;
            var def = viewPartFactory.FindTypeDefinition(typeId);
            if (def == null)
                return;
            var viewPart = def.Create();
            AddViewPart(viewPart);
            viewPart.OnLoadLayout(ele);
        }

        void ILayoutManagerService.SaveLayout(XElement parent)
        {
            var fileName = GetFileName();
            var ele = parent.Elements(ELE_FILE)
                .FirstOrDefault(x => x.Attribute(ATTR_FILENAME)?.Value == fileName);
            ele?.Remove();
            ele = new XElement(ELE_FILE,
                new XAttribute(ATTR_FILENAME, fileName),
                new XAttribute(ATTR_ZOOM, shellVm.Zoom),
                new XAttribute(ATTR_OFFSET, shellVm.Offset)
                );
            parent.Add(ele);
            foreach (var viewPart in ViewParts)
            {
                SaveViewPart(ele, viewPart);
            }
        }

        private void SaveViewPart(XElement parent, ViewPart viewPart)
        {
            var def = viewPartFactory.FindDefinition(viewPart);
            var ele = parent.Elements(ELE_VIEWPART)
                .FirstOrDefault(e => e.Attribute(ATTR_TYPE)?.Value == def.TypeId);
            if (ele != null)
                ele.Remove();
            var (vpc, index) = GetViewPartControl(viewPart);
            ele = new XElement(ELE_VIEWPART,
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
