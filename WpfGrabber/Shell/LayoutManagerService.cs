using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{

    public class LayoutManagerService
    {
        private const string ELE_LAYOUTS = "Layouts";
        private const string ELE_FILE = "File";
        private const string ATTR_FILENAME = "FileName";
        private const string ATTR_ZOOM = "Zoom";
        private const string ATTR_OFFSET = "Offset";
        private const string ELE_VIEWPART = "View";
        private const string ATTR_TYPE = "Type";
        private const string ATTR_WIDTH = "Width";
        private const string ELE_NAMEDLAYOUT = "Save";
        private const string ATTR_NAME = "Name";
        private const string ELE_LAYOUT = "Layout";
        private readonly ShellVm shellVm;
        private readonly IViewPartServiceEx viewPartService;
        private readonly ViewPartFactory viewPartFactory;

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

        public void LoadLayoutFile(string name)
        {
            var fileName = GetConfigFileName();
            if (!File.Exists(fileName))
                return;
            var doc = XDocument.Load(fileName);
            if (doc.Root.Name != ELE_LAYOUTS)
                throw new FormatException("Invalid xml file");
            LoadLayout(doc.Root, name);
        }

        public void SaveLayoutFile(string name)
        {
            SaveLayoutFileInternal(name, remove: false);
        }

        public void RemoveLayoutFile(string name)
        {
            SaveLayoutFileInternal(name, remove: true);
        }

        public void LoadLayout(XElement parent, string name)
        {
            var fileName = GetProjectFileName();
            var ele = parent.Elements(ELE_FILE)
                .FirstOrDefault(x => x.Attribute(ATTR_FILENAME)?.Value == fileName);
            if (ele == null)
                return;
            viewPartService.RemoveAll();

            var source = ele;
            if (!string.IsNullOrEmpty(name))
            {
                source = ele.Elements(ELE_NAMEDLAYOUT)
                    .FirstOrDefault(e => e.Attribute(ATTR_NAME)?.Value == name);
                if (source == null)
                    return;
            }
            else
            {
                shellVm.Layouts.AddRange(source.Elements(ELE_NAMEDLAYOUT).Select(e => e.Attribute(ATTR_NAME)?.Value), clear: true);
            }
            shellVm.Zoom = (double?)source.Attribute(ATTR_ZOOM) ?? shellVm.Zoom;
            shellVm.Offset = (int?)source.Attribute(ATTR_OFFSET) ?? 0;
            foreach (var e in source.Elements(ELE_VIEWPART))
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
            var width = (int?)ele.Attribute(ATTR_WIDTH);
            viewPartService.SetOptions(viewPart, new ViewPartOptions() { Title = def.Title, Width = width??0 });
            var le = ele.Element(ELE_LAYOUT);
            if (le != null)
                viewPart.OnLoadLayout(le);
        }

        private void SaveLayoutFileInternal(string name, bool remove)
        {
            var fileName = GetConfigFileName();
            var doc = File.Exists(fileName) ? XDocument.Load(fileName) : new XDocument(new XElement(ELE_LAYOUTS));

            SaveLayoutInternal(doc.Root, name, remove);
            doc.Save(fileName);
        }

        public void SaveLayout(XElement parent, string name)
        {
            SaveLayoutInternal(parent, name, remove: false);
        }

        private void SaveLayoutInternal(XElement parent, string name, bool remove)
        {
            var fileName = GetProjectFileName();
            var ele = parent.Elements(ELE_FILE)
                .FirstOrDefault(x => x.Attribute(ATTR_FILENAME)?.Value == fileName);
            if (ele == null)
            {
                ele = new XElement(ELE_FILE, new XAttribute(ATTR_FILENAME, fileName));
                parent.Add(ele);
            }
            var target = ele;
            if (!String.IsNullOrEmpty(name))
            {
                target = ele.Elements(ELE_NAMEDLAYOUT)
                    .FirstOrDefault(x => x.Attribute(ATTR_NAME)?.Value == name);
                if (target == null)
                {
                    target = new XElement(ELE_NAMEDLAYOUT, new XAttribute(ATTR_NAME, name));
                    ele.Add(target);
                }
                if (!shellVm.Layouts.Contains(name))
                    shellVm.Layouts.Add(name);
            }
            if (remove)
            {
                shellVm.Layouts.Remove(name);
                target.Remove();
                return;
            }
            target.SetAttributeValue(ATTR_ZOOM, shellVm.Zoom);
            target.SetAttributeValue(ATTR_OFFSET, shellVm.Offset);
            //remove closed viewparts
            var vpElements = target.Elements(ELE_VIEWPART)
                .Select(e => new { ele = e, type = e.Attribute(ATTR_TYPE)?.Value })
                .Select(a => new { a.ele, a.type, def = viewPartFactory.FindTypeDefinition(a.type) })
                .ToArray();
            var viewParts = viewPartService
                .ViewParts
                .Select(vp => new { vp, def = viewPartFactory.FindDefinition(vp) })
                .ToArray();
            foreach (var item in vpElements
                .Where(a => !viewParts.Any(b => a.def == b.def)))
            {
                item.ele.Remove();
            }

            foreach (var viewPart in viewPartService.ViewParts)
            {
                SaveViewPart(target, viewPart);
            }
        }

        private void SaveViewPart(XElement parent, ViewPart viewPart)
        {
            var def = viewPartFactory.FindDefinition(viewPart);
            var ele = parent.Elements(ELE_VIEWPART)
                .FirstOrDefault(e => e.Attribute(ATTR_TYPE)?.Value == def.TypeId);
            if (ele != null)
                ele.Remove();
            var (vpc, index, width) = viewPartService.GetViewPartControl(viewPart);
            ele = new XElement(ELE_VIEWPART,
                new XAttribute(ATTR_TYPE, def.TypeId)
                );
            if (width != 0)
                ele.Add(new XAttribute(ATTR_WIDTH, width));
            parent.Add(ele);
            var le = ele.Element(ELE_LAYOUT);
            if (le == null)
            {
                le = new XElement(ELE_LAYOUT);
                ele.Add(le);
            }
            viewPart.OnSaveLayout(le);
        }

    }
}
