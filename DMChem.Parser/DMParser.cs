using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using Superpower;
using Superpower.Parsers;
using Superpower.Model;
using System;
using System.Drawing;

namespace DMChem.Parser
{
    public static class DMParser
    {
        public static Dictionary<string, object> Defines = new Dictionary<string, object>();

        public static readonly TokenListParser<DMToken, string> SubPath =
            from _ in Token.EqualTo(DMToken.Slash)
            from chars in Token.EqualTo(DMToken.Identifier)
            select chars.Span.ToString();

        public static readonly TokenListParser<DMToken, string> ReferencePath =
             from first in Token.EqualTo(DMToken.Identifier).Optional()
             from rest in SubPath.Try().AtLeastOnce()
             from __ in Token.EqualTo(DMToken.Slash).Optional()
             select string.Join("/", first == null ? rest : rest.Prepend(first.Value.ToStringValue()));

        public static readonly TokenListParser<DMToken, string> Identifier =
            ReferencePath
                .Or(Token.EqualTo(DMToken.Identifier)
                    .Select(i => i.Span.ToString()));

        public static readonly TokenListParser<DMToken, decimal> Number =
             Token.EqualTo(DMToken.NumericLiteral)
                .Apply(Numerics.DecimalDecimal);

        public static readonly TokenListParser<DMToken, bool> Boolean =
            Token.EqualTo(DMToken.BooleanLiteral).Select(x => bool.Parse(x.Span.ToString()));

        public static readonly TokenListParser<DMToken, string> String =
            Token.EqualTo(DMToken.StringLiteral).Select(s => s.Span.ToString().Trim('"'));

        public static readonly TokenListParser<DMToken, string[]> Union =
            Identifier.AtLeastOnceDelimitedBy(Token.EqualTo(DMToken.Bar));

        public static readonly TokenListParser<DMToken, Color> Rgb =
            from _ in Token.EqualTo(DMToken.RgbKeyword)
            from __ in Token.EqualTo(DMToken.OpenParen)
            from rgb in Number.ManyDelimitedBy(Token.EqualTo(DMToken.Comma)).Where(a => a.Length == 3).Select(a => a.Select(c => (int)c).ToArray())
            from ___ in Token.EqualTo(DMToken.CloseParen)
            select Color.FromArgb(rgb[0], rgb[1], rgb[2]);

        public static readonly TokenListParser<DMToken, Color> HexColor =
            Token.EqualTo(DMToken.StringLiteral)
            .Select(s => s.Span.Skip(1).First(s.Span.Length - 2).ToStringValue())
            .Where(s => s.StartsWith("#"))
            .Select(ColorTranslator.FromHtml);

        public static readonly TokenListParser<DMToken, KeyValuePair<object, object>> DictPair =
            from key in Parse.Ref(() => Expr)
            from _ in Token.EqualTo(DMToken.Equals)
            from value in Parse.Ref(() => Expr)
            select new KeyValuePair<object, object>(key, value);

        public static readonly TokenListParser<DMToken, Dictionary<object, object>> DictList =
            from _ in Token.EqualTo(DMToken.ListKeyword)
            from __ in Token.EqualTo(DMToken.OpenParen)
            from pairs in DictPair.ManyDelimitedBy(Token.EqualTo(DMToken.Comma))
            from ___ in Token.EqualTo(DMToken.CloseParen)
            select pairs.ToDictionary(p => p.Key, p => p.Value);

        public static readonly TokenListParser<DMToken, decimal> Multiply =
            from a in Number
            from _ in Token.EqualTo(DMToken.Asterisk)
            from b in Number.Or(Parse.Ref(() => Expr).Where(b => b is decimal).Select(b => (decimal)b))
            select a * b;

        public static readonly TokenListParser<DMToken, object> StaticVariableReference =
            from id in Identifier.Where(id => Defines.ContainsKey(id))
            select Defines[id];

        public static readonly TokenListParser<DMToken, object> Expr =
            from value in
                Rgb.Select(c => c as object)
                .Or(Union.Where(a => a.Length > 1).Try().Select(u => u as object))
                .Or(Multiply.Try().Select(m => m as object))
                .Or(StaticVariableReference)
                .Or(Identifier.Select(r => r as object))
                .Or(Number.Select(n => n as object))
                .Or(HexColor.Select(c => c as object))
                .Or(String.Select(s => s as object))
                .Or(DictList.Select(d => d as object))
                .Or(Boolean.Select(d => d as object))
            select value;

        public static readonly TokenListParser<DMToken, KeyValuePair<string, object>> Assignment =
            from variableName in Identifier
            from _ in Token.EqualTo(DMToken.Equals)
            from value in Expr
            select new KeyValuePair<string, object>(variableName, value);

        public static readonly TokenListParser<DMToken, KeyValuePair<string, object>> Define =
            from _ in Token.EqualTo(DMToken.Hash)
            from __ in Token.EqualTo(DMToken.Define)
            from key in Token.EqualTo(DMToken.Identifier).Select(id => id.ToStringValue())
            from value in Expr
            select new KeyValuePair<string, object>(key, value);

        public static readonly TokenListParser<DMToken, KeyValuePair<string, dynamic>> Object =
            from path in ReferencePath
            from _ in Token.EqualTo(DMToken.Eol)
            from values in
                Token.EqualTo(DMToken.LeadWhitespace)
                .IgnoreThen(Assignment).Try()
                .ManyDelimitedBy(Token.EqualTo(DMToken.Eol))
                .Select(a =>
                {
                    var eo = new ExpandoObject();
                    var eoColl = eo as IDictionary<string, object>;

                    foreach (var item in a)
                    {
                        eoColl.Add(item);
                    }
                    return new KeyValuePair<string, dynamic>(path, eo as dynamic);
                })
            select values;

        public static readonly TokenListParser<DMToken, Dictionary<string, dynamic>> ObjectList =
            from _ in Token.EqualTo(DMToken.Eol).Many()
            from objs in (
                from obj in Object.Or(Define)
                from __ in Token.EqualTo(DMToken.Eol).Or(Token.EqualTo(DMToken.LeadWhitespace)).Many()
                select obj)
                .Many().Select(KvpsToDic).AtEnd()
            select objs;

        private static Dictionary<string, dynamic> KvpsToDic(KeyValuePair<string, dynamic>[] kvps)
        {
            var dic = new Dictionary<string, dynamic>();
            foreach (var kvp in kvps)
            {
                dic.Add(kvp.Key, kvp.Value);
            }
            return dic;
        }

        public static bool DynHas(dynamic dyn, string valName)
        {
            IDictionary<string, object> properties = dyn as IDictionary<string, object>;
            return properties.ContainsKey(valName);
        }
    }
}
