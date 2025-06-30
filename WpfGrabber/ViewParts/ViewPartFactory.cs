using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.ViewParts
{
    public class ViewPartFactory
    {
        private List<ViewPartDef> _definitions = new List<ViewPartDef>();
        public IEnumerable<ViewPartDef> Definitions => _definitions;
        public ViewPartFactory()
        {
            _definitions.AddRange(new ViewPartDef[] {
                new ViewPartDef<FileMapViewPart>(){Title = "File map"},
                new ViewPartDef<Binary8BitViewPart>(){Title = "Binary 8bit" },
                new ViewPartDef<HexDumpViewPart>(){Title = "Hex dump" },
                new ViewPartDef<MaskedImagesViewPart>(){Title = "Binary masked images"},
                new ViewPartDef<FontBinaryViewPart>(){Title = "Font"},
                new ViewPartDef<EngineViewPart>(){Title = "Engine"},
                new ViewPartDef<Z80DumpViewPart>(){Title = "Z80 disasm"},
                ImageSpriteViewPart.Def,
                new ViewPartDef<ImageSheetViewPart>(){Title = "Image sheet cutter"},
                //whatsnew
                //errors
                new ViewPartDef<TestViewPart>(){Title = "(Test)"},
            });
        }

        public ViewPartDef FindTypeDefinition(string typeId)
        {
            return Definitions.FirstOrDefault(x => x.TypeId == typeId);
        }
    }
    public static class ViewPartExtensions
    {
        public static ViewPartDef FindDefinition(this ViewPartFactory factory, ViewPart viewPart)
        {
            return factory.FindTypeDefinition(viewPart.GetType().Name);
        }
        public static ViewPartDef FindDefinition<T>(this ViewPartFactory factory) where T : ViewPart
        {
            return factory.FindTypeDefinition(typeof(T).Name);
        }

    }

    public abstract class ViewPartDef
    {
        public string Title { get; set; }
        public virtual Type ViewPartType { get; }
        public string TypeId => ViewPartType.Name;
        public abstract ViewPart Create();
    }
    public class ViewPartDef<T> : ViewPartDef where T : ViewPart, new()
    {
        public override Type ViewPartType => typeof(T);
        public override ViewPart Create() => new T();
    }

}
