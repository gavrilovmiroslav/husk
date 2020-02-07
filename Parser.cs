using System;
using System.Collections.Generic;
using System.Linq;

namespace husk
{
    public class Parser
    {
        public Parser() { }

        public bool Apply(List<Token> code, out Environment env)
        {
            LoadSequenceStack(DiscardComments(code));
            env = new Environment();

            try
            {
                while(lookaheadFirst.TokenType != TokenType.Terminator)
                    Statement();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

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

        private void Statement()
        {
            switch(lookaheadFirst.TokenType)
            {
                case TokenType.DeclTypeKeyword: DeclType(); break;
                default: break;
            }
        }

        private void DeclType()
        {
            Discard(TokenType.DeclTypeKeyword);

            var newtype = GetTypeDef();
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
            new DeclType() { declaredType = newtype, constructors = constructors };
        }

        private TypeDef GetTypeDef()
        {
            var list = IdentStream();

            SimpleType t = new SimpleType();
            if (list.Count > 1)
            {
                t.name = list[0];
                t.args = list.GetRange(1, list.Count - 1).ToArray();
            }
            else
            {
                t.name = list[0];
                t.args = null;
            };

            return t;
        }

        private DataDef GetDataDef(TypeDef t)
        {
            var list = IdentStream();

            DataDef d = new DataDef();
            d.type = t;

            if (list.Count > 1)
            {
                d.name = list[0];
                d.args = list.GetRange(1, list.Count - 1).ToArray();
            }
            else
            {
                d.name = list[0];
                d.args = null;
            };

            return d;
        }

        private List<string> IdentStream()
        {
            var typename = Expect(TokenType.Identifier);
            var arglist = new List<string>();
            Next();

            while(lookaheadFirst.TokenType == TokenType.Identifier)
            {
                arglist.Add(Expect(TokenType.Identifier).Value);
                Next();
            }

            return arglist.Prepend(typename.Value).ToList<string>();
        }

        #endregion
    }
}