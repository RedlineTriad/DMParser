using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace DMChemParser
{
    public class DMTokenizer : Tokenizer<DMToken>
    {
        static readonly Dictionary<char, DMToken> singleCharacterTokens = new Dictionary<char, DMToken>
        {
            ['='] = DMToken.Equals,
            ['('] = DMToken.OpenParen,
            [')'] = DMToken.CloseParen,
            [','] = DMToken.Comma,
            ['.'] = DMToken.Dot,
        };

        static readonly Dictionary<string, DMToken> keywords = new Dictionary<string, DMToken>
        {
            ["list"] = DMToken.ListKeyword,
            ["for"] = DMToken.ForKeyword,
            ["<="] = DMToken.LessThanEquals,
            ["++"] = DMToken.PlusPlus,
        };

        protected override IEnumerable<Result<DMToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            while (next.HasValue)
            {
                next = SkipWhiteSpace(next);
                if(!next.HasValue){
                    yield break;
                }
                if (next.Value == '/')
                {
                    var pos = next.Location;
                    next = next.Remainder.ConsumeChar();
                    if (next.Value == '/')
                    {
                        while (next.Value != '\n')
                        {
                            next = next.Remainder.ConsumeChar();
                        }
                    }
                    else
                    {
                        yield return Result.Value(DMToken.Slash, pos, next.Location);
                        if (TryIndentifier(ref next, out var identifier))
                        {
                            yield return identifier;
                        }
                        while (next.Value == '/')
                        {
                            yield return Result.Value(DMToken.Slash, next.Location, next.Remainder);
                            next = next.Remainder.ConsumeChar();
                            if (TryIndentifier(ref next, out identifier))
                            {
                                yield return identifier;
                            }
                        }
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
                else if (next.Value == '"')
                {
                    var subPath = new StringBuilder();
                    var subPathStart = next.Location;
                    next = next.Remainder.ConsumeChar();
                    while (next.Value != '"')
                    {
                        subPath.Append(next.Value);
                        next = next.Remainder.ConsumeChar();
                    }
                    next = next.Remainder.ConsumeChar();
                    yield return Result.Value(DMToken.StringLiteral, subPathStart, next.Location);
                }
                else if (char.IsDigit(next.Value))
                {
                    var integer = Numerics.Integer(next.Location);
                    next = integer.Remainder.ConsumeChar();
                    yield return Result.Value(DMToken.NumericLiteral, integer.Location, integer.Remainder);
                }
            }
        }

        private static bool TryIndentifier(ref Result<char> next, out Result<DMToken> value)
        {
            if (next.HasValue && char.IsLetter(next.Value))
            {
                var identifierStart = next.Location;
                bool firstLetter = false;
                while (char.IsLetter(next.Value) || (firstLetter && (next.Value == '_' || char.IsDigit(next.Value))))
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
            var maxLength = Math.Min(next.Location.Length, keywords.Max(keyword => keyword.Key.Length));
            var start = next.Location;

            for (int i = 1; i < maxLength; i++)
            {
                var text = next.Location.First(i);
                if (keywords.TryGetValue(text.ToString(), out var keyword))
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

        private static Result<char> SkipWhiteSpace(Result<char> next)
        {
            if (char.IsWhiteSpace(next.Value))
            {
                next = next.Remainder.ConsumeChar();
            }

            return next;
        }
    }
}
