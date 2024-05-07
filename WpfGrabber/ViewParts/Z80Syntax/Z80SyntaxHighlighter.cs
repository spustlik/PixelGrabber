using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using WpfGrabber.Readers.Z80;
using System.Windows.Media;

namespace WpfGrabber.ViewParts.Z80Syntax
{
    public class Z80SyntaxHighlighter
    {
        private const string LANGNAME = "Z80Dump";

        private static bool _initialized = false;

        public static void Init()
        {
#if DEBUGXSHD
            var path = Path.Combine(Tools.DeploymentHelper.GetApplicationPath(), "../../ViewPart/Z80Syntax/Z80.xshd");
            if (File.Exists(path))
            {
                using( var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    ReadHighlighter("ActionLang", s);
                }
                _initialized = false;
            }
#endif
            if (_initialized)
                return;
            var identity = typeof(Z80SyntaxHighlighter);
            lock (identity)
            {
                if (_initialized)
                    return;
                var identityParts = identity.FullName.Split('.');
                var identityPrefix = String.Join(".", identityParts.Take(identityParts.Length - 1));
                foreach (var name in identity.Assembly.GetManifestResourceNames())
                {
                    if (name.StartsWith(identityPrefix, StringComparison.Ordinal))
                    {
                        var highlightingName = name.Split('.').Reverse().Skip(1).First();
                        ReadHighlighter(highlightingName, identity.Assembly.GetManifestResourceStream(name));
                    }
                }
                var lang = GetDefinition();
                var keywords = GetEnumValues<Z80Op>();
                AddKewords(lang, true, keywords.Select(k => k.ToString()));

                var registers = GetEnumValues<Z80Register>();
                AddKewords(lang, true, registers.Select(k => k.ToString()), "Register");

                _initialized = true;
            }
        }

        private static T[] GetEnumValues<T>() where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        private static void AddKewords(
            IHighlightingDefinition lang,
            bool caseSensitive,
            IEnumerable<string> keywords,
            string color = null)
        {
            if (!keywords.Any())
                return;
            keywords = keywords.OrderByDescending(k => k.Length).ThenBy(x => x).ToArray();
            var options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled;
            if (!caseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }
            lang.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Color = lang.GetNamedColor(color ?? "Keyword"),
                    Regex = new Regex(@"\b(?>" + string.Join("|", keywords) + @")\b", options)
                });
        }

        public static IHighlightingDefinition GetDefinition()
        {
            return HighlightingManager.Instance.GetDefinition(LANGNAME);
        }

        private static void ReadHighlighter(string language, Stream resource)
        {
            using (var reader = XmlReader.Create(resource))
            {
                var xshd = HighlightingLoader.LoadXshd(reader);
                var highlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting(language, xshd.Extensions.ToArray(), highlighting);
            }
        }

    }
}
