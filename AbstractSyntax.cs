using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace husk
{
    public class Curry : INode
    {
        public List<INode> Seq = new List<INode>();

        private Curry() {}

        public Curry(IEnumerable<INode> ts, params INode[] rest)
        {
            if(ts != null)
                Seq.AddRange(ts);
            Seq.AddRange(rest);
        }

        public INode Head
        {
            get
            {
                return Seq[0];
            }
        }

        public Curry Tail
        {
            get
            {
                if (this is Arrow)
                    return new Arrow(Seq.GetRange(1, Seq.Count - 1).ToList<INode>().ToArray());
                else if (this is Apply)
                    return new Apply(Seq.GetRange(1, Seq.Count - 1).ToList<INode>().ToArray());
                else throw new Exception("No other curried forms");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Curry other)
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

        public override INode Subst(INode key, INode substitution)
        {
            if (this.Equals(key)) return substitution;

            if (this is Arrow arr)
            {
                return new Arrow(null, arr.Seq.Select(item => item.Subst(key, substitution)).ToList<INode>().ToArray());
            }
            else if (this is Apply app)
            {
                return new Apply(null, app.Seq.Select(item => item.Subst(key, substitution)).ToList<INode>().ToArray());
            }
            else 
                return this;
        }

        public override object Clone()
        {
            if (this is Arrow arr)
            {
                return new Arrow(null, arr.Seq.Select(item => (INode)item.Clone()).ToList<INode>().ToArray());
            }
            else if (this is Apply app)
            {
                return new Apply(null, app.Seq.Select(item => (INode)item.Clone()).ToList<INode>().ToArray());
            }
            else throw new Exception("No other curried forms found.");
        }
    }

    public class Rel : INode
    {
        public INode Left { get; }
        public INode Right { get; }

        public Rel(INode l, INode r)
        {
            Left = l;
            Right = r;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rel other)
            {
                if (!Left.Equals(other.Left)) return false;
                if (!Right.Equals(other.Right)) return false;

                return true;
            }
            return base.Equals(obj);
        }

        public override object Clone()
        {
            return new Rel(Left, Right);
        }

        public override INode Subst(INode key, INode substitution)
        {
            if (this.Equals(key))
                return substitution;
            else
                return new Rel(Left.Subst(key, substitution), Right.Subst(key, substitution));
        }
    }

    public abstract class INode : ICloneable
    {
        public abstract object Clone();
        public abstract INode Subst(INode key, INode substitution);

        public static Arrow operator+(INode self, INode other)
        {
            if (self is Arrow arr)
            {
                return new Arrow(arr.Seq, other);
            }
            else
            {
                return new Arrow(null, self, other);
            }
        }

        public static Apply operator-(INode self, INode other)
        {
            if (self is Apply arr)
            {
                return new Apply(arr.Seq, other);
            }
            else
            {
                return new Apply(null, self, other);
            }
        }
    }

    public class Arrow : Curry
    {
        public Arrow(IEnumerable<INode> ts, params INode[] rest) : base(ts, rest) { }

        public override string ToString() => String.Join(" -> ", this.Seq);
    }

    public class Builtin : INode
    {
        public IsA Decl { get; }

        public Builtin(IsA typed)
        {
            Decl = typed;
        }

        public override object Clone()
        {
            return new Builtin(Decl);
        }

        public override INode Subst(INode key, INode substitution)
        {
            return this;
        }
    }

    public class IsA : Rel
    {
        public IsA(INode left, INode right) : base(left, right) { }

        public override string ToString() => $"{Left} : {Right}";
    }

    public class Equals : Rel
    {
        public Equals(INode left, INode right) : base(left, right) { }

        public override string ToString() => $"{Left} = {Right}";
    }

    public class FuncDef : Rel
    {
        public FuncDef(INode left, INode right) : base(left, right) { }

        public override string ToString() => $"{Left} = {Right}";
    }

    public class Apply : Curry
    {
        public Apply(IEnumerable<INode> ts, params INode[] rest) : base(ts, rest) { }

        public override string ToString() => String.Join(" ", this.Seq);
    }

    public class Entity : Curry
    {
        public Entity(IEnumerable<INode> ts, params INode[] rest) : base(ts, rest) { }

        public override string ToString() => String.Join(" ", this.Seq);
    }

    public class Marker : INode
    {        
        public Marker() {}

        public override string ToString() => $"<>";

        public override object Clone()
        {
            return new Marker();
        }

        public override INode Subst(INode key, INode substitution)
        {
            return this;
        }
    }

    public class BuiltinMarker : INode
    {
        public BuiltinMarker() { }

        public override string ToString() => $"<b>";

        public override object Clone()
        {
            return new BuiltinMarker();
        }

        public override INode Subst(INode key, INode substitution)
        {
            return this;
        }
    }

    public class Comp : INode
    {
        public INode Inside { get; }

        public Comp(INode node)
        {
            Inside = node;
        }

        public override string ToString() => $"{Inside}";

        public override bool Equals(object obj)
        {
            if (obj is Comp other)
                return Inside.Equals(other.Inside);
            else
                return base.Equals(obj);
        }

        public override object Clone()
        {
            return new Comp(Inside);
        }

        public override INode Subst(INode key, INode substitution)
        {
            if (this.Equals(key)) 
                return substitution;
            else 
                return new Comp(Inside.Subst(key, substitution));
        }
    }

    public class Id : INode
    {
        public string Name { get; }

        public Id(string name)
        {
            Name = name;
        }

        public override string ToString() => $"{Name}";

        public override bool Equals(object obj)
        {
            if (obj is Id other)
                return other.Name == Name;
            else
                return base.Equals(obj);
        }

        public override object Clone()
        {
            return new Id(Name);
        }

        public override INode Subst(INode key, INode substitution)
        {
            if (this.Equals(key))
                return substitution;
            else
                return this;
        }
    }
}
