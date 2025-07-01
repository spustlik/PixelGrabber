using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WpfGrabber
{
    public abstract class SimpleDataObjectEx : SimpleDataObject
    {
        //public string Dirty => Computed(() => IsChanged ? "*" : "");
        //public string TestComputed2 => Computed(() => "abcd"); //no MemberExpression
        //public string TestComputed => Computed(() => this.TestEnum + "-" + this.TestBool); //Root is this
        //public string TestComputed2 => Computed(() => new Point(3, 4).X + ""); ..Root expression is New! probably also new {} as NewInit


        protected T Computed<T>(Expression<Func<T>> getter, [CallerMemberName] string propertyName = null)
        {
            //this will be called on every deps change
            var ex = new MembersExtractor();
            ex.Visit(getter);
            var deps = ex.Members.Select(m => m.Path).Distinct().ToArray();

            var compiled = getter.Compile();
            this.PropertyChanged += (s, e) =>
            {
                if (deps.Contains(e.PropertyName))
                    DoPropertyChanged(propertyName);
                //TODO: deep path , if we have a property like A.B.C, then we need to check if A.B or A.B.C changed
                //also collections/arrays
                //but inner objects are not supported with layout saver

            };

            return compiled();
        }

    }

    class MembersExtractor : ExpressionVisitor
    {
        public class MemberAccessInfo
        {
            public MemberAccessInfo(MemberExpression node)
            {
                this.Member = node.Member;
                this.Expression = node.Expression;
                var path = node.Member.Name;
                var ex = node.Expression;
                while (ex is MemberExpression me)
                {
                    ex = me.Expression;
                    path = $"{me.Member.Name}.{path}";
                }
                if (!(ex is ConstantExpression cex))
                    throw new NotSupportedException($"Member expression {ex.NodeType} is not supported, only constant");
                Root = cex.Value;
                Path = path;
            }

            public MemberInfo Member { get; }
            public Expression Expression { get; }
            public string Path { get; }
            public object Root { get; }
        }
        public List<MemberAccessInfo> Members { get; } = new List<MemberAccessInfo>();
        protected override Expression VisitMember(MemberExpression node)
        {
            Members.Add(new MemberAccessInfo(node));
            return base.VisitMember(node);
        }
    }
}
