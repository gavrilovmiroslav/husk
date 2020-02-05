using System;
using System.Collections.Generic;
using System.Text;
using Spart.Actions;
using Spart.Debug;
using Spart.Parsers;
using Spart.Parsers.NonTerminal;
using Spart.Parsers.Primitives;
using Spart.Scanners;
using static Spart.Parsers.Ops;
using static Spart.Parsers.Prims;

namespace husk
{
    public class HuskParser
    {
		Parser language;
		Stack<INode> stack;

		public HuskParser()
		{
			static StringParser str(string s) => new StringParser(s);
		
			var _ = ZeroOrMore(WhiteSpace);
			var __ = OneOrMore(WhiteSpace);

			var whatever = new Rule("Whatever", '_')[OnWhatever];
			var identifier = new Rule("Identifier", Ops.Sequence(Optional('\''), Letter, 
								ZeroOrMore(LetterOrDigit | '-' | '_')))[OnIdentifier];
			var typeTerm = new Rule("Type Term");   // 
			var typeFactor = new Rule("Type Factor");
			var typeExpression = new Rule("Type Expression");
			var typeGroup = new Rule("Type Group");

			var typeComp = Sequence(__, typeFactor)[OnComposition];
			var arrow = Sequence(_, str("->"), _, typeTerm)[OnArrow];

			typeGroup.Parser = Sequence('(', typeExpression, ')');
			typeFactor.Parser = typeGroup | identifier;
			typeTerm.Parser = Sequence(typeFactor, ZeroOrMore(typeComp));
			typeExpression.Parser = Sequence(typeTerm, ZeroOrMore(arrow));

			var hole = new Rule("Hole Keyword", '?');
			var builtin = new Rule("Builtin Keyword", str("builtin"));
			var funclet = new Rule("Let Keyword", str(">"))[OnLet];
			var decltype = new Rule("Decltype Keyword", str("decltype"))[OnDeclType];

			var isa = new Rule("Is A", Sequence(':', _, typeExpression))[OnIsA];
			var typeAssertion = Sequence(Optional(Sequence(builtin, _)), identifier, _, isa);

			var statement = new Rule("Statement");

			var funcDeclaration = Sequence(funclet, __, identifier, OneOrMore(Sequence(__, identifier | whatever)), _, '=', _, statement)[OnFuncDecl];
			var typeDeclaration = Sequence(decltype, __, identifier, ZeroOrMore(Sequence(__, identifier)))[OnTypeDecl];

			var factor = new Rule("Factor");
			var expression = new Rule("Expression");
			var group = new Rule("Group");
			var composition = Sequence(__, typeFactor)[OnComposition];
			group.Parser = Sequence('(', typeExpression, ')');
			factor.Parser = group | identifier;
			expression.Parser = Sequence(factor, ZeroOrMore(composition));

			language = ZeroOrMore(Sequence(typeAssertion | funcDeclaration | typeDeclaration, Optional(OneOrMore(Sequence(_, ';', _)))));
			//language = typeExpression;
		}

		private void OnBlock(object sender, ActionEventArgs args)
		{
			Console.WriteLine("Hole: " + args.Value);
			stack.Push(new Id("?"));
		}

		private void OnWhatever(object sender, ActionEventArgs args)
		{
			Console.WriteLine("Whatever: " + args.Value);
			stack.Push(new Id("_"));
		}

		private void OnIdentifier(object sender, ActionEventArgs args)
		{
			Console.WriteLine("Id: " + args.Value);
			stack.Push(new Id(args.Value));
		}

		private void OnBuiltin(object sender, ActionEventArgs args)
		{
			Console.WriteLine("Builtin: " + args.Value);
			stack.Push(new BuiltinMarker());
		}

		private void OnDeclType(object sender, ActionEventArgs args)
		{
			Console.WriteLine("Decltype: " + args.Value);
			stack.Push(new Marker());
		}

		private void OnTypeDecl(object sender, ActionEventArgs args)
		{
			List<INode> fargs = new List<INode>();
			while (true)
			{
				var top = stack.Pop();
				if (top is Marker _)
				{
					break;
				}
				else fargs.Add(top);
			}

			fargs.Reverse();
			var e = new Entity(null, fargs.ToArray());
			stack.Push(e);
		}

		private void OnFuncDecl(object sender, ActionEventArgs args)
		{
			List<INode> fargs = new List<INode>();
			while(true)
			{
				var top = stack.Pop();
				if (top is Marker _)
				{
					break;
				}
				else fargs.Add(top);
			}

			fargs.Reverse();

			stack.Push(new FuncDef(new Apply(null, fargs.GetRange(0, fargs.Count - 1).ToArray()), fargs.GetRange(fargs.Count - 1, 1)[0]));
		}

		private void OnLet(object sender, ActionEventArgs args)
		{
			stack.Push(new Marker());
		}

		private void OnParens(object sender, ActionEventArgs args)
		{
			Console.WriteLine("OnParens: " + args.Value);
			stack.Push(new Comp(stack.Pop()));
		}

		private void OnComposition(object sender, ActionEventArgs args)
		{
			Console.WriteLine("App: " + args.Value);
			var rhs = stack.Pop();
			var lhs = stack.Pop();

			if(lhs is Apply aid)
			{				
				stack.Push(new Apply(aid.Seq, rhs));
			}
			else
			{
				stack.Push(new Apply(null, lhs, rhs));
			}
		}

		private void OnIsA(object sender, ActionEventArgs args)
		{
			Console.WriteLine("Is a: " + args.Value);
			var rhs = stack.Pop();
			var lhs = stack.Pop();

			if (stack.Count > 0 && stack.Peek() is BuiltinMarker _)
			{
				stack.Pop();
				stack.Push(new Builtin(new IsA(lhs, rhs)));
			}
			else
			{
				stack.Push(new IsA(lhs, rhs));
			}
		}

		private void OnArrow(object sender, ActionEventArgs args)
		{
			Console.WriteLine("Arrow: " + args.Value);
			var rhs = stack.Pop();
			var lhs = stack.Pop();

			stack.Push(new Arrow(null, lhs, rhs));
		}

		public List<INode> Parse(String s)
		{
			stack = new Stack<INode>();
			StringScanner sc = new StringScanner(s);
			this.language.Parse(sc);
			
			if (!sc.AtEnd)
				return null;
			
			var list = new List<INode>(stack);
			list.Reverse();

			Console.WriteLine("-------------------");
			foreach (var t in list) Console.WriteLine(t);
			Console.WriteLine("-------------------");
			return list;
		}
	}
}
