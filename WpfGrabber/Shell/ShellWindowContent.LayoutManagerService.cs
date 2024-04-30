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
        void LoadLayoutFile();
        void SaveLayoutFile();
    }

    public class LayoutManagerService : ILayoutManagerService
    {
        private const string ELE_LAYOUTS = "Layouts";
        private const string ELE_FILE = "File";
        private const string ATTR_FILENAME = "FileName";
        private const string ATTR_ZOOM = "Zoom";
        private const string ATTR_OFFSET = "Offset";
        private const string ELE_VIEWPART = "View";
        private const string ATTR_TYPE = "Type";
        private const string ATTR_WIDTH = "Width";
        private const string ELE_LAYOUT = "Layout";
        private ShellVm shellVm;
        private IViewPartServiceEx viewPartService;
        private ViewPartFactory viewPartFactory;

        public LayoutManagerService(
            ShellVm shellVm, 
            IViewPartServiceEx viewPartService,
            ViewPartFactory viewPartFactory)
        {
            this.shellVm = shellVm;
            this.viewPartService = viewPartService;
            this.viewPartFactory = viewPartFactory;
        }
        private string GetProjectFileName()
        {
            return Path.GetFileName(shellVm.FileName).ToUpperInvariant();
        }

        private string GetConfigFileName()
        {
            return Path.Combine(Path.GetDirectoryName(shellVm.FileName), ".pixelgrabber");
        }

        public void LoadLayoutFile()
        {
            var fileName = GetConfigFileName();
            if (!File.Exists(fileName))
                return;
            var doc = XDocument.Load(fileName);
            if (doc.Root.Name != ELE_LAYOUTS)
                throw new FormatException("Invalid xml file");
            LoadLayout(doc.Root);
        }

        public void SaveLayoutFile()
        {
            var fileName = GetConfigFileName();
            var doc = File.Exists(fileName) ? XDocument.Load(fileName) : new XDocument(new XElement(ELE_LAYOUTS));

            SaveLayout(doc.Root);
            doc.Save(fileName);
        }

        private void LoadLayout(XElement parent)
        {
            var fileName = GetProjectFileName();
            var ele = parent.Elements(ELE_FILE)
                .FirstOrDefault(x => x.Attribute(ATTR_FILENAME)?.Value == fileName);
            if (ele == null)
                return;
            viewPartService.RemoveAll();
            shellVm.Zoom = (double?)ele.Attribute(ATTR_ZOOM) ?? shellVm.Zoom;
            shellVm.Offset = (int?)ele.Attribute(ATTR_OFFSET) ?? 0;
            foreach (var e in ele.Elements(ELE_VIEWPART))
            {
                LoadViewPart(e);
            }
            viewPartService.FixGridLayout();
        }

        private void LoadViewPart(XElement ele)
        {
            var typeId = ele.Attribute(ATTR_TYPE)?.Value;
            var def = viewPartFactory.FindTypeDefinition(typeId);
            if (def == null)
                return;
            var viewPart = def.Create();
            viewPartService.Add(viewPart);
            var width = (int)ele.Attribute(ATTR_WIDTH);
            viewPartService.SetOptions(viewPart, new ViewPartOptions() { Title = def.Title, Width = width });
            var le = ele.Element(ELE_LAYOUT);
            if (le != null)
                viewPart.OnLoadLayout(le);
        }

        private void SaveLayout(XElement parent)
        {
            var fileName = GetProjectFileName();
            var ele = parent.Elements(ELE_FILE)
                .FirstOrDefault(x => x.Attribute(ATTR_FILENAME)?.Value == fileName);
            ele?.Remove();
            ele = new XElement(ELE_FILE,
                new XAttribute(ATTR_FILENAME, fileName),
                new XAttribute(ATTR_ZOOM, shellVm.Zoom),
                new XAttribute(ATTR_OFFSET, shellVm.Offset)
                );
            parent.Add(ele);
            foreach (var viewPart in viewPartService.ViewParts)
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
            var (vpc, index) = viewPartService.GetViewPartControl(viewPart);
            ele = new XElement(ELE_VIEWPART,
                new XAttribute(ATTR_TYPE, def.TypeId),
                new XAttribute(ATTR_WIDTH, (int)vpc.ActualWidth)
                );
            parent.Add(ele);
            var le = new XElement(ELE_LAYOUT);
            viewPart.OnSaveLayout(le);
            ele.Add(le);
        }

    }
}
