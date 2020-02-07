using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace husk
{
    public enum TokenType
    {
        DeclTypeKeyword,
        Identifier,
        Equals,
        OpenParenthesis,
        ClosedParenthesis,
        Follows,
        Implies,
        Belongs,
        StringLiteral,
        Operator,
        Comma,
        AliasKeyword,
        MatchKeyword,
        CaseKeyword,
        NumericLiteral,
        Comment,
        Empty,
        CommentLine,
        Or,
        Terminator,
        Invalid,
        Semicolon
    }

    public class TokenMatch
    {
        public bool IsMatch { get; set; }
        public TokenType TokenType { get; set; }
        public string Value { get; set; }
        public string Remaining { get; set; }
    }

    public class TokenDefinition
    {
        private Regex regex;
        private readonly TokenType returning;

        public TokenDefinition(TokenType returnsToken, string pattern)
        {
            this.regex = new Regex(pattern, RegexOptions.IgnoreCase);
            this.returning = returnsToken;
        }

        public TokenMatch Match(string input)
        {
            var match = this.regex.Match(input);
            if(match.Success)
            {
                string remainingText = string.Empty;
                if (match.Length != input.Length)
                    remainingText = input.Substring(match.Length);

                return new TokenMatch()
                {
                    IsMatch = true,
                    Remaining = remainingText,
                    TokenType = returning,
                    Value = match.Value
                };
            }
            else
            {
                return new TokenMatch() { IsMatch = false, Remaining = input };
            }
        }
    }

    public class Token : ICloneable
    {
        public Token(TokenType tokenType)
        {
            TokenType = tokenType;
            Value = string.Empty;
        }

        public Token(TokenType tokenType, string value)
        {
            TokenType = tokenType;
            Value = value;
        }

        public TokenType TokenType { get; set; }
        public string Value { get; set; }

        public object Clone()
        {
            return new Token(TokenType, Value);
        }

        public override string ToString()
        {
            return $"({TokenType}, {Value})";
        }
    }

    public class Tokenizer
    {
        List<TokenDefinition> definitions = new List<TokenDefinition>();
        Dictionary<TokenType, Func<string, string>> correctors = new Dictionary<TokenType, Func<string, string>>();

        public Tokenizer()
        {
            definitions.Add(new TokenDefinition(TokenType.CommentLine, "^--[^\n]*\n"));
            definitions.Add(new TokenDefinition(TokenType.Empty, "^\\s*\n"));
            definitions.Add(new TokenDefinition(TokenType.DeclTypeKeyword, "^decltype"));
            definitions.Add(new TokenDefinition(TokenType.AliasKeyword, "^alias"));
            definitions.Add(new TokenDefinition(TokenType.MatchKeyword, "^match"));
            definitions.Add(new TokenDefinition(TokenType.CaseKeyword, "^case"));
            definitions.Add(new TokenDefinition(TokenType.Follows, "^->"));
            definitions.Add(new TokenDefinition(TokenType.Implies, "^=>"));
            definitions.Add(new TokenDefinition(TokenType.Belongs, "^:"));
            definitions.Add(new TokenDefinition(TokenType.Or, "^\\|"));
            definitions.Add(new TokenDefinition(TokenType.ClosedParenthesis, "^\\)"));
            definitions.Add(new TokenDefinition(TokenType.OpenParenthesis, "^\\("));
            definitions.Add(new TokenDefinition(TokenType.Equals, "^="));
            definitions.Add(new TokenDefinition(TokenType.Comma, "^,"));
            definitions.Add(new TokenDefinition(TokenType.Semicolon, "^;"));
            definitions.Add(new TokenDefinition(TokenType.Identifier, "^['a-z]{1}[a-zA-Z0-9_\\-]*"));
            definitions.Add(new TokenDefinition(TokenType.StringLiteral, @"^""[^""]*"""));
            definitions.Add(new TokenDefinition(TokenType.Operator, "^[+\\-*/~$\\.\\^&><]{1}"));
            definitions.Add(new TokenDefinition(TokenType.NumericLiteral, "^\\d+"));
            definitions.Add(new TokenDefinition(TokenType.Comment, "^{{[^}}]*}}"));

            correctors[TokenType.CommentLine] = (input) => input.Substring(2).Trim();
            correctors[TokenType.Comment] = (input) => input.Substring(2, input.Length - 4).Trim();
        }

        public bool Apply(string text, out List<Token> tokens)
        {
            tokens = new List<Token>();

            string remainingText = text;

            while(!string.IsNullOrWhiteSpace(remainingText))
            {
                var match = FindMatch(remainingText.TrimStart());
                if(match.IsMatch)
                {
                    var token = new Token(match.TokenType, match.Value);

                    if (correctors.ContainsKey(token.TokenType))
                        token.Value = correctors[token.TokenType](token.Value);

                    tokens.Add(token);
                    remainingText = match.Remaining;
                } 
                else
                {
                    var invalidTokenMatch = CreateInvalidTokenMatch(remainingText);
                    tokens.Add(new Token(invalidTokenMatch.TokenType, invalidTokenMatch.Value));
                    remainingText = invalidTokenMatch.Remaining;
                }
            }

            tokens.Add(new Token(TokenType.Terminator, string.Empty));
            return true;
        }

        private TokenMatch FindMatch(string text)
        {
            foreach (var tokenDefinition in definitions)
            {
                var match = tokenDefinition.Match(text);
                if (match.IsMatch)
                    return match;
            }

            return new TokenMatch() { IsMatch = false };
        }

        private TokenMatch CreateInvalidTokenMatch(string text)
        {
            var match = Regex.Match(text, "(^\\S+\\s)|^\\S+");
            if (match.Success)
            {
                return new TokenMatch()
                {
                    IsMatch = true,
                    Remaining = text.Substring(match.Length),
                    TokenType = TokenType.Invalid,
                    Value = match.Value.Trim()
                };
            }

            throw new Exception("Failed to generate invalid token");
        }
    }
}
