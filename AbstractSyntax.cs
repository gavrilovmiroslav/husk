using System;
using System.Collections.Generic;
using System.Text;

namespace husk
{
    public class Curry<T> : INode
    {
        public List<T> Seq = new List<T>();

        private Curry() {}

        public Curry(IEnumerable<T> ts, params T[] rest)
        {
            if(ts != null)
                Seq.AddRange(ts);
            Seq.AddRange(rest);
        }

        public T Head
        {
            get
            {
                return Seq[0];
            }
        }

        public Curry<T> Tail
        {
            get
            {
                return new Curry<T>(Seq.GetRange(1, Seq.Count - 1));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Curry<T> other)
            {
                if (Seq.Count != other.Seq.Count) return false;
                for (int i = 0; i < Seq.Count; i++)
                {
                    if (!Seq[i].Equals(other.Seq[i])) return false;
                }

                return true;
            }
            return base.Equals(obj);
        }
    }

    public class Rel<T> : INode
    {
        public T Left { get; }
        public T Right { get; }

        public Rel(T l, T r)
        {
            Left = l;
            Right = r;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rel<T> other)
            {
                if (!Left.Equals(other.Left)) return false;
                if (!Right.Equals(other.Right)) return false;

                return true;
            }
            return base.Equals(obj);
        }

    }

    public interface INode { }

    public class Arrow : Curry<INode>
    {
        public Arrow(IEnumerable<INode> ts, params INode[] rest) : base(ts, rest) { }

        public override string ToString() => "(" + String.Join(" -> ", this.Seq) + ")";
    }

    public class Builtin : INode
    {
        public IsA Decl { get; }

        public Builtin(IsA typed)
        {
            Decl = typed;
        }
    }

    public class IsA : Rel<INode>
    {
        public IsA(INode left, INode right) : base(left, right) { }

        public override string ToString() => $"({Left} : {Right})";
    }

    public class Equals : Rel<INode>
    {
        public Equals(INode left, INode right) : base(left, right) { }

        public override string ToString() => $"({Left} = {Right})";
    }

    public class FuncDef : Rel<INode>
    {
        public FuncDef(INode left, INode right) : base(left, right) { }

        public override string ToString() => $"({Left} = {Right})";
    }

    public class Apply : Curry<INode>
    {
        public Apply(IEnumerable<INode> ts, params INode[] rest) : base(ts, rest) { }

        public override string ToString() => "(" + String.Join(" ", this.Seq) + ")";
    }

    public class Entity : Curry<INode>
    {
        public Entity(IEnumerable<INode> ts, params INode[] rest) : base(ts, rest) { }

        public override string ToString() => "(" + String.Join(" ", this.Seq) + ")";
    }

    public class Marker : INode
    {        
        public Marker() {}

        public override string ToString() => $"<>";
    }

    public class BuiltinMarker : INode
    {
        public BuiltinMarker() { }

        public override string ToString() => $"<b>";
    }

    public class Comp : INode
    {
        public INode Inside { get; }

        public Comp(INode node)
        {
            Inside = node;
        }

        public override string ToString() => $"[{Inside}]";

        public override bool Equals(object obj)
        {
            if (obj is Comp other)
                return Inside.Equals(other.Inside);
            else
                return base.Equals(obj);
        }
    }

    public class Id : INode
    {
        public string Name { get; }

        public Id(string name)
        {
            Name = name;
        }

        public override string ToString() => $"({Name})";

        public override bool Equals(object obj)
        {
            if (obj is Id other)
                return other.Name == Name;
            else
                return base.Equals(obj);
        }
    }
}
