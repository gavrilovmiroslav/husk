using System;
using System.Collections.Generic;
using System.Linq;

namespace husk
{
    public class Parser
    {
        public Parser()
        {
        }

        public void Parse(string code)
        {
            throw new NotImplementedException();
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

            // prepare lookahead
            lookaheadFirst = stack.Pop();
            lookaheadSecond = stack.Pop();
        }

        private Token Expect(TokenType type)
        {
            if (lookaheadFirst.TokenType != type)
                throw new Exception($"Expected {type} but found: {lookaheadFirst.TokenType}");

            return lookaheadFirst;
        }

        private void Discard()
        {
            lookaheadFirst = (Token)lookaheadSecond.Clone();

            if(stack.Any())
        }
    }
}