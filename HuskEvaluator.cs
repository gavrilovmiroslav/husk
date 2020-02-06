using System;
using System.Collections.Generic;
using System.Text;
using static husk.ConsoleUtilities;
using System.Linq;

namespace husk
{
    public class HuskEvaluator
    {
        Dictionary<string, Entity> types = new Dictionary<string, Entity>();
        Dictionary<string, List<FuncDef>> patterns = new Dictionary<string, List<FuncDef>>();
        Dictionary<string, INode> defs = new Dictionary<string, INode>();

        static int callDepth = 1;

        public INode Call(INode name, params INode[] args)
        {
            Console.WriteLine(new String(' ', callDepth) + "Function call: " + name + " (" + String.Join(',', args.Select(x => $"{x}")) + ")");
            callDepth++;

            if (name is Id id)
            {
                if (patterns.ContainsKey(id.Name))
                {
                    var funcPatterns = patterns[id.Name];

                    INode selectedPattern = null;
                    var subst = new Dictionary<INode, INode>();

                    foreach (var pattern in funcPatterns)
                    {
                        if (selectedPattern != null) break;
                        if (pattern.Left is Apply app)
                        {
                            var head = app.Head;
                            var parameters = app.Tail;

                            var broken = false;
                            subst.Clear();

                            for (int i = 0; i < parameters.Seq.Count; i++)
                            {
                                var p = parameters.Seq[i];
                                var a = Refactor(args[i]);

                                if (p is Id pid)
                                {
                                    if (pid.Name == "_") // Whatever
                                        continue;

                                    else if (Char.IsUpper(pid.Name[0])) // var
                                    {   
                                        subst.Add(pid, a);
                                    }
                                    else // value
                                    {
                                        if (!pid.Equals(a))
                                        {
                                            broken = true;
                                        }
                                    }
                                }
                            }

                            if (!broken)
                            {
                                selectedPattern = pattern;
                                break;
                            }
                            else
                            {
                                subst.Clear();
                                broken = false;
                            }
                        }
                    }

                    if (selectedPattern != null && selectedPattern is FuncDef f)
                    {
                        var refined = f.Right;
                        foreach (var key in subst.Keys)
                        {
                            refined = refined.Subst(key, subst[key]);
                        }

                        callDepth--;
                        Console.WriteLine(new String(' ', callDepth) + "Function return: " + refined);
                        return refined;
                    }
                    else
                    {
                        throw new Exception($"Cannot find matching function call for {name} {args}!");
                    }
                }
                else
                {
                    throw new Exception($"No function called {name} found!");
                }
            }

            return null;
        }

        private INode Refactor(INode input)
        {
            if (input is Apply app && app.Seq.Count == 1) return Refactor(app.Seq[0]);
            else if (input is Apply fn) return Refactor(Call(fn.Head, fn.Tail.Seq.ToArray()));
            else if (input is Comp comp) return Refactor(comp.Inside);
            else return input;
        }

        public INode Interpret()
        {
            Console.WriteLine("\n\n==============================");
            Console.WriteLine("  HUSK Interpreter running...");
            Console.WriteLine("==============================\n\n");

            if (defs.ContainsKey("main"))
            {
                if (defs["main"] is Arrow arr
                    && arr.Seq.Count == 2
                    && arr.Tail.Head is Id returnType
                    && returnType.Equals(new Id("'unit")))
                {
                    INode result = new Apply(null, new Id("main"), new Id("true"));
                    do
                    {
                        if(result is Apply app)
                        {
                            result = Refactor(Call(app.Head, app.Tail.Seq.ToArray()));
                        }
                    } while (!(result is Id));
                    Console.WriteLine(result);
                    Console.WriteLine("\n\n==============================\n\n");
                }
                else
                {
                    if (!(defs["main"] is Arrow _))
                    {
                        PushColor(ConsoleColor.Red);
                        Console.WriteLine(defs["main"].GetType());
                        Console.WriteLine($"#1 <main> has to have type t -> 'unit, but {defs["main"]} found!");
                        PopColor();
                    }
                    else if (defs["main"] is Arrow a && a.Seq.Count != 2)
                    {
                        PushColor(ConsoleColor.Red);
                        Console.WriteLine($"#2 <main> has to have type t -> 'unit, but {a} found!");
                        PopColor();
                    }
                }
            }

            return null;
        }

        public bool Evaluate(List<INode> tokens) {
            foreach (var token in tokens)
            {
                if (token is Entity e)
                {
                    types.Add(e.Head.ToString(), e);
                } 
                else if (token is FuncDef fn && fn.Left is Apply app)
                {
                    var head = app.Head.ToString();
                    if (!patterns.ContainsKey(head)) patterns.Add(head, new List<FuncDef>());
                    patterns[head].Add(fn);
                }
                else if (token is IsA isa && isa.Left is Id name)
                {
                    defs.Add(name.Name, isa.Right);
                } else if (token is Builtin b && b.Decl is IsA decl && decl.Left is Id id)
                {
                    defs.Add(id.Name, decl.Right);
                }
            }

            return true;
        }
    }
}
