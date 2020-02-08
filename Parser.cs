using System;
using System.Collections.Generic;
using System.Linq;

namespace husk
{
    public class Parser
    {
        private Environment env;
        private int depth;

        public Parser() { }

        public bool Apply(List<Token> code, out Environment env)
        {
            LoadSequenceStack(DiscardComments(code));
            this.env = new Environment();
            this.depth = 0;

            try
            {
                while(lookaheadFirst.TokenType != TokenType.Terminator)
                    Statement();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                env = this.env;
                return false;
            }

            env = this.env;
            return true;
        }

        private Stack<Token> stack;
        private Token lookaheadFirst;
        private Token lookaheadSecond;
 
        private void LoadSequenceStack(List<Token> tokens)
        {
            stack = new Stack<Token>();
            int count = tokens.Count;
            for (int i = count - 1; i >= 0; i--)
                stack.Push(tokens[i]);

            lookaheadFirst = stack.Pop();
            lookaheadSecond = stack.Pop();
        }

        private Token Expect(TokenType type)
        {
            if (lookaheadFirst.TokenType != type)
                throw new Exception($"Expected {type} but found: {lookaheadFirst.TokenType}");

            return lookaheadFirst;
        }

        private void Next() => Discard();

        private void Discard()
        {
            lookaheadFirst = (Token)lookaheadSecond.Clone();

            if (stack.Any())
                lookaheadSecond = stack.Pop();
            else
                lookaheadSecond = new Token(TokenType.Terminator, string.Empty);
        }

        private void Discard(TokenType type)
        {
            if (lookaheadFirst.TokenType != type)
                throw new Exception($"Expected {type} but found: {lookaheadFirst.TokenType}");

            Discard();
        }

        #region Parsing

        private List<Token> DiscardComments(List<Token> input)
        {
            List<Token> output = new List<Token>();

            foreach(var token in input)
            {
                if (token.TokenType == TokenType.Comment 
                 || token.TokenType == TokenType.CommentLine)
                    continue;

                output.Add(token);
            }

            return output;
        }

        private List<Token> StreamIdentifiers()
        {
            var typename = Expect(TokenType.Identifier);
            var arglist = new List<Token>();
            Next();

            while (lookaheadFirst.TokenType == TokenType.Identifier)
            {
                arglist.Add(Expect(TokenType.Identifier));
                Next();
            }

            return arglist.Prepend(typename).ToList<Token>();
        }

        private Morphism StreamMorphisms()
        {
            var name = Expect(TokenType.Identifier);
            var arglist = new List<Morphism>();
            Next();

            while (lookaheadFirst.TokenType == TokenType.Identifier 
                || lookaheadFirst.TokenType == TokenType.OpenParenthesis)
            {
                if(lookaheadFirst.TokenType == TokenType.OpenParenthesis)
                {
                    arglist.Add(TryQuote(StreamMorphisms));
                }
                else
                {
                    arglist.Add(new Morphism() { name = Expect(TokenType.Identifier).Value });
                    Next();
                }
            }

            return new Morphism() { name = name.Value, ids = arglist.ToArray() };
        }

        private void Statement()
        {
            switch(lookaheadFirst.TokenType)
            {
                case TokenType.DeclTypeKeyword: DeclareType(); break;
                case TokenType.Identifier:
                    if (lookaheadSecond.TokenType == TokenType.Belongs)
                        DeclareFunction();
                    
                    break;
                default: throw new Exception($"Statement expected, found: {lookaheadFirst}");
            }
        }

        private void DeclareType()
        {
            Discard(TokenType.DeclTypeKeyword);

            var newtype = GetSimpleTypeSignature();
            Discard(TokenType.Equals);

            var constructors = new List<DataDef>();

            while(lookaheadFirst.TokenType != TokenType.Semicolon)
            {
                constructors.Add(GetDataDef(newtype));

                if(lookaheadFirst.TokenType == TokenType.Or)
                    Discard();
            }
            Discard();

            Console.WriteLine($"Declaring type {newtype}");
            env.typeDeclarations.Add(newtype.ToString(), new DeclType() { declaredType = newtype, constructors = constructors });
        }

        private void DeclareFunction()
        {
            var funcName = Expect(TokenType.Identifier);
            Next();

            Discard(TokenType.Belongs);

            var funcType = GetFunctionTypeSignature();
            funcType.name = funcName.Value;

            Discard(TokenType.WhereKeyword);

            GetFunctionPatterns(ref funcType);
            Console.WriteLine($"Declaring function {funcType}");
            env.funcDeclarations.Add(funcName.Value, funcType);
        }

        private T TryQuote<T>(Func<T> unquote)
            where T : class
        {
            if (lookaheadFirst.TokenType == TokenType.OpenParenthesis)
            {
                depth++;
                Discard(TokenType.OpenParenthesis);
                var result = unquote();

                Discard(TokenType.ClosedParenthesis);
                depth--;

                return result;
            }
            else
                return null;
        }

        private TypeDef GetSimpleTypeSignature()
        {
            var list = new List<TypeDef>();
            GetSimpleTypeSignature(ref list);
            return list[0];
        }

        private void GetSimpleTypeSignature(ref List<TypeDef> list)
        {
            if (lookaheadFirst.TokenType == TokenType.OpenParenthesis)
            {
                var unquoted = TryQuote(GetFunctionTypeSignature);
                if (unquoted != null)
                {
                    list.Add(unquoted);
                }
            }
            else if (lookaheadFirst.TokenType == TokenType.Identifier)
            {
                var ids = StreamIdentifiers();

                SimpleType t = new SimpleType();
                if (list.Count > 1)
                {
                    t.name = ids[0].Value;
                    t.args = ids.GetRange(1, ids.Count - 1).Select(x => x.Value).ToArray();
                }
                else
                {
                    t.name = ids[0].Value;
                    t.args = null;
                };

                list.Add(t);
            }
            else
                throw new Exception($"Expected parens or identifier, found: {lookaheadFirst}");
        }

        private FunctionType GetFunctionTypeSignature()
        {
            var list = new List<TypeDef>();

            GetSimpleTypeSignature(ref list);
            while (lookaheadFirst.TokenType == TokenType.Follows)
            {
                Next();
                GetSimpleTypeSignature(ref list);
            }

            return new FunctionType() { types = list.ToArray() };
        }

        private DataDef GetDataDef(TypeDef t)
        {
            var list = StreamIdentifiers();

            DataDef d = new DataDef();
            d.type = t;

            if (list.Count > 1)
            {
                d.name = list[0].Value;
                d.args = list.GetRange(1, list.Count - 1).Select(x => x.Value).ToArray();
            }
            else
            {
                d.name = list[0].Value;
                d.args = null;
            };

            return d;
        }

        private void GetFunctionPatterns(ref FunctionType func)
        {
            var list = new List<FunctionPattern>();
            do
            {
                var funcName = Expect(TokenType.Identifier);
                if (func.name != funcName.Value)
                    throw new Exception($"Expected a pattern for function {func.name}, found: {funcName}");

                int parameterCount = func.types.Length;

                var parameters = StreamMorphisms();

                Discard(TokenType.Equals);

                var body = StreamMorphisms();
                list.Add(new FunctionPattern()
                {
                    func = func,
                    parameters = parameters,
                    body = body
                });

                if (lookaheadFirst.TokenType == TokenType.Comma) Next();
                else if (lookaheadFirst.TokenType == TokenType.Semicolon) break;
            } while (true);

            func.patterns = list.ToArray();
            Discard(TokenType.Semicolon);
        }

        #endregion
    }
}