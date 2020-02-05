using System;
using System.Collections.Generic;
using System.Text;
using static husk.ConsoleUtilities;

namespace husk
{
    public class HuskInterpreter
    {
        Dictionary<string, Entity> types = new Dictionary<string, Entity>();
        Dictionary<string, List<FuncDef>> cases = new Dictionary<string, List<FuncDef>>();
        Dictionary<string, INode> values = new Dictionary<string, INode>();

        public void Interpret(List<INode> tokens) {
            foreach (var token in tokens)
            {
                if (token is Entity e)
                {
                    types.Add(e.Head.ToString(), e);
                    Console.WriteLine($"\nTYPE {e} DECLARED UNDER KEY {e.Head.ToString()}!");
                } 
                else if (token is FuncDef fn && fn.Left is Apply app)
                {
                    var head = app.Head.ToString();
                    if (!cases.ContainsKey(head)) cases.Add(head, new List<FuncDef>());
                    cases[head].Add(fn);
                    Console.WriteLine($"FUNC {fn} DECLARED UNDER KEY {app.Head.ToString()}!");
                }
                else if (token is IsA isa && isa.Left is Id name)
                {
                    values.Add(name.Name, isa.Right);
                    Console.WriteLine($"\nVALUE {name} DECLARED WITH TYPE {isa.Right}!");
                } else if (token is Builtin b && b.Decl is IsA decl && decl.Left is Id id)
                {
                    values.Add(id.Name, decl.Right);
                    Console.WriteLine($"\nBUILTIN {id} DECLARED WITH TYPE {decl.Right}!");
                }
            }

            if (values.ContainsKey("main"))
            {
                var mainType = new Arrow(null, new Id("'unit"), new Id("'unit"));

                if (values["main"] is Arrow arr
                    && arr.Seq.Count == 2
                    && arr.Tail.Head is Id returnType
                    && returnType.Equals(new Id("'unit")))
                {
                    Console.WriteLine("Calling main!");
                }
                else
                {
                    if (!(values["main"] is Arrow _))
                    {
                        PushColor(ConsoleColor.Red);
                        Console.WriteLine(values["main"].GetType());
                        Console.WriteLine($"#1 <main> has to have type t -> 'unit, but {values["main"]} found!");
                        PopColor();
                    }
                    else if (values["main"] is Arrow a && a.Seq.Count != 2)
                    {
                        PushColor(ConsoleColor.Red);
                        Console.WriteLine($"#2 <main> has to have type t -> 'unit, but {a} found!");
                        PopColor();
                    }

                    return;
                }
            }
        }
    }
}
