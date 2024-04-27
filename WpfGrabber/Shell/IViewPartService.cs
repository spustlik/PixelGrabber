﻿using WpfGrabber.ViewParts;

namespace WpfGrabber.Shell
{
    public interface IViewPartService
	{
		void Add(ViewPart viewPart);
		void Remove(ViewPart viewPart);
		void RemoveAll();
		void SetOptions(ViewPart viewPart, ViewPartOptions options);
	}

    public class ViewPartOptions
    {
        public string Title { get; set; }
        public int Width { get; set; }
    }


    public static class ViewPartServiceExtensions
    {
        public static void AddNewPart(this IViewPartService svc, ViewPartDef def)
        {
            var part = def.Create();
            svc.Add(part);
            svc.SetOptions(part, new ViewPartOptions() { Title = def.Title });

        }
    }
}