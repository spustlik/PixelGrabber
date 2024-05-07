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
                new ViewPartDef<Binary8BitViewPart>(){Title = "Binary 8bit" },
                new ViewPartDef<HexDumpViewPart>(){Title = "Hex dump" },
                new ViewPartDef<MaskedImagesViewPart>(){Title = "Binary masked images"},
                new ViewPartDef<FontBinaryViewPart>(){Title = "Font"},
                new ViewPartDef<EngineViewPart>(){Title = "Engine"},
                new ViewPartDef<Z80DumpViewPart>(){Title = "Z80 disasm"},
                new ViewPartDef<ImageSpriteViewPart>(){Title = "Image sprite sheet"},
                new ViewPartDef<TestViewPart>(){Title = "(Test)"},
            });
        }

        public ViewPartDef FindDefinition(ViewPart viewPart)
        {
            return FindTypeDefinition(viewPart.GetType().Name);
        }

        public ViewPartDef FindTypeDefinition(string typeId)
        {
            return Definitions.FirstOrDefault(x => x.TypeId == typeId);
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
