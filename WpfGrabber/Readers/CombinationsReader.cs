using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfGrabber.Readers
{
    public class CombinationsReader
    {

        public class ParsedLine
        {
            public int Line { get; set; }
            public string Text { get; set; }
            public string Name { get; set; }
            public string[] Values { get; set; }
        }

        public ParsedLine[] Combinations { get; private set; }
        public IEnumerable<ParsedLine> Read(string text)
        {
            var result = new List<ParsedLine>();
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var s = lines[i];
                s = s.Trim();
                if (s.StartsWith("#") || String.IsNullOrEmpty(s))
                    continue;
                var parts = s.Split(new[] { ':' }, 2);
                var values = parts.Length == 1 ? new string[0] : parts[1].Split(',').Select(v => v.Trim()).ToArray();
                var r = new ParsedLine() { Line = i, Text = lines[i], Name = parts[0].Trim(), Values = values };
                result.Add(r);
            }
            Combinations = result.ToArray();
            return result;
        }

        public List<string> Check(IEnumerable<string> knownNames)
        {
            var result = new List<string>();
            foreach (var item in Combinations)
            {
                if (knownNames.Contains(item.Name))
                    result.Add($"Already exists: {item.Name}");
            }
            foreach (var item in Combinations)
            {
                foreach (var v in item.Values)
                {
                    if (!knownNames.Contains(v))
                    {
                        result.Add($"Unknown: {v} at {item.Line + 1}");
                    }
                }
            }
            return result;
        }

        public IEnumerable<string> Combine()
        {
            //c1:a,b,c
            //c2:d,e,f
            //c3:a,e,f
            //c4:g,h,i,j

            //--> ada, ade, adf,  aea, aee, aef, ...

            IEnumerable<List<string>> IterateC(int ci)
            {
                for (int vi = 0; vi < Combinations[ci].Values.Length; vi++)
                {
                    if (ci == Combinations.Length - 1)
                    {
                        var r = new List<string>();
                        r.Insert(0, Combinations[ci].Values[vi]);
                        yield return r;
                    }
                    else
                    {
                        var r = IterateC(ci + 1);
                        foreach (var item in r)
                        {
                            item.Insert(0, Combinations[ci].Values[vi]);
                            yield return item;
                        }
                    }
                }
            }

            var combs = IterateC(0);
            var result = combs.Select(x => String.Join(",", x.Distinct())).Distinct();
            return result;
        }
    }
}
