using System.Collections.Generic;
using System.Linq;
using WpfGrabber.Controls;
using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    public interface IViewPartService
    {
        void Add(ViewPart viewPart);
        void Remove(ViewPart viewPart);
        void RemoveAll();
        void SetOptions(ViewPart viewPart, ViewPartOptions options);
    }

    public interface IViewPartServiceEx : IViewPartService
    {
        IEnumerable<ViewPart> ViewParts { get; }

        void FixGridLayout();

        (ViewPartControl vpc, int index, int width) GetViewPartControl(ViewPart viewPart);
    }
    public class ViewPartOptions
    {
        public string Title { get; set; }
        public int? Width { get; set; }
    }


    public static class ViewPartServiceExtensions
    {
        public static T GetOrCreate<T>(this IViewPartServiceEx svc, ViewPartDef<T> def) where T : ViewPart, new()
        {
            var vp = svc.ViewParts.OfType<T>().FirstOrDefault();
            if (vp == null)
            {
                vp = svc.AddAndCreate(def);
            }
            return vp;

        }
        public static T AddAndCreate<T>(this IViewPartService svc, ViewPartDef<T> def) where T : ViewPart, new()
        {
            var vp = svc.AddNewPart(def);
            return (T)vp;
        }

        public static ViewPart AddNewPart(this IViewPartService svc, ViewPartDef def)
        {
            var part = def.Create();
            svc.Add(part);
            svc.SetOptions(part, new ViewPartOptions() { Title = def.Title });
            return part;
        }
    }
}
