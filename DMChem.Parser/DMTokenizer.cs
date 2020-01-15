using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace DMChem.Parser
{
    public class DMTokenizer : Tokenizer<DMToken>
    {
        private static readonly Dictionary<char, DMToken> singleCharacterTokens = new Dictionary<char, DMToken>
        {
            ['='] = DMToken.Equals,
            ['!'] = DMToken.Exclamation,
            ['('] = DMToken.OpenParen,
            [')'] = DMToken.CloseParen,
            ['['] = DMToken.OpenBracket,
            [']'] = DMToken.CloseBracket,
            [','] = DMToken.Comma,
            ['.'] = DMToken.Dot,
            ['+'] = DMToken.Plus,
            ['-'] = DMToken.Minus,
            ['*'] = DMToken.Asterisk,
            ['/'] = DMToken.Slash,
            ['<'] = DMToken.LessThan,
            ['>'] = DMToken.GreaterThan,
        };

        private static readonly Dictionary<string, DMToken> keywords = new Dictionary<string, DMToken>
        {
            ["list"] = DMToken.ListKeyword,
            ["for"] = DMToken.ForKeyword,
            ["in"] = DMToken.InKeyword,
            ["to"] = DMToken.ToKeyword,
            ["if"] = DMToken.IfKeyword,
            ["else"] = DMToken.ElseKeyword,
            ["new"] = DMToken.NewKeyword,
            ["TRUE"] = DMToken.TrueKeyword,
            ["FALSE"] = DMToken.FalseKeyword,
            ["=="] = DMToken.EqualsEquals,
            ["<="] = DMToken.LessThanEquals,
            [">="] = DMToken.GreaterThanEquals,
            ["<="] = DMToken.LessThanEquals,
            ["++"] = DMToken.PlusPlus,
            ["+="] = DMToken.PlusEquals,
            ["-="] = DMToken.MinusEquals,
            ["*="] = DMToken.AsteriskEquals,
            ["/="] = DMToken.SlashEquals,
            ["&"] = DMToken.Ampersand,
            ["&&"] = DMToken.AmpersandAmpersand,
            ["|"] = DMToken.Bar,
            ["||"] = DMToken.BarBar,
        };

        private static readonly HashSet<char> stringLiteralOpeners = new HashSet<char> { '"', '\'' };

        private static readonly HashSet<char> whitespace = new HashSet<char> { ' ', '\t' };

        protected override IEnumerable<Result<DMToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            var lastFail = next.Location;
            while (next.HasValue)
            {
                if (SkipWhiteSpace(ref next, DMToken.LeadWhitespace, out var whitespace))
                {
                    yield return whitespace;
                }
                if (!next.HasValue)
                {
                    yield break;
                }
                if (next.Location.Length >= 2 && next.Location.First(2).ToString() == "//")
                {
                    while (next.Value != '\n')
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                }
                else if (TryKeyword(ref next, out var keyword))
                {
                    yield return keyword;
                }
                else if (TryIndentifier(ref next, out var identifier))
                {
                    yield return identifier;
                }
                else if (singleCharacterTokens.TryGetValue(next.Value, out var charToken))
                {
                    yield return Result.Value(charToken, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else if (TryStringLiteral(ref next, out var stringLiteral))
                {
                    yield return stringLiteral;
                }
                else if (char.IsDigit(next.Value))
                {
                    var integer = Numerics.Integer(next.Location);
                    next = integer.Remainder.ConsumeChar();
                    yield return Result.Value(DMToken.NumericLiteral, integer.Location, integer.Remainder);
                }
                else if (lastFail == next.Location)
                {
                    break;
                }
                else
                {
                    lastFail = next.Location;
                }
                SkipWhiteSpace(ref next, DMToken.TrailWhitespace, out _);
                if (next.Value == '\n')
                {
                    yield return Result.Value(DMToken.Eol, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
            }
        }

        private static bool TryIndentifier(ref Result<char> next, out Result<DMToken> value)
        {
            if (next.HasValue && (char.IsLetter(next.Value) || next.Value == '_'))
            {
                var identifierStart = next.Location;
                bool firstLetter = false;
                while (char.IsLetter(next.Value) || next.Value == '_' || (firstLetter && char.IsDigit(next.Value)))
                {
                    next = next.Remainder.ConsumeChar();
                    firstLetter = true;
                }
                value = Result.Value(DMToken.Identifier, identifierStart, next.Location);
                return true;
            }
            value = default;
            return false;
        }

        private static bool TryKeyword(ref Result<char> next, out Result<DMToken> value)
        {
            var remainingLength = next.Location.Length;
            var maxWordLength = Math.Min(remainingLength, keywords.Max(keyword => keyword.Key.Length));
            var fetchLength = Math.Min(remainingLength, maxWordLength + 1);
            var start = next.Location;

            for (int i = fetchLength - 1; i > 0; i--)
            {
                var text = next.Location.First(i + 1).ToString();
                var wordText = text.Substring(0, i);
                var lastChar = text.Last();
                if (keywords.TryGetValue(wordText, out var keyword) && (i == fetchLength || !char.IsLetter(lastChar)))
                {
                    for (int j = 0; j < i; j++)
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    value = Result.Value(keyword, start, next.Location);
                    return true;
                }
            }
            value = default;
            return false;
        }

        private static bool TryStringLiteral(ref Result<char> next, out Result<DMToken> value)
        {
            if (stringLiteralOpeners.Contains(next.Value))
            {
                var opener = next.Value;
                var subPath = new StringBuilder();
                var subPathStart = next.Location;
                next = next.Remainder.ConsumeChar();
                while (next.Value != opener)
                {
                    subPath.Append(next.Value);
                    next = next.Remainder.ConsumeChar();
                }
                next = next.Remainder.ConsumeChar();
                value = Result.Value(DMToken.StringLiteral, subPathStart, next.Location);
                return true;
            }
            value = default;
            return false;
        }

        private static bool SkipWhiteSpace(ref Result<char> next, DMToken token, out Result<DMToken> value)
        {
            if (whitespace.Contains(next.Value))
            {
                var start = next.Location;
                while (whitespace.Contains(next.Value))
                {
                    next = next.Remainder.ConsumeChar();
                }
                value = Result.Value(token, start, next.Location);
                return true;
            }
            value = default;
            return false;
        }
    }
}
