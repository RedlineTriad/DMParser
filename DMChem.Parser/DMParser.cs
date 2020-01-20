using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using Superpower;
using Superpower.Parsers;
using Superpower.Model;

namespace DMChem.Parser
{
    public static class DMParser
    {
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

        public static readonly TokenListParser<DMToken, int> Integer =
             Token.EqualTo(DMToken.NumericLiteral)
                .Apply(Numerics.IntegerInt32);

        public static readonly TokenListParser<DMToken, bool> Boolean =
            Token.EqualTo(DMToken.BooleanLiteral).Select(x => bool.Parse(x.Span.ToString()));

        public static readonly TokenListParser<DMToken, string> String =
            Token.EqualTo(DMToken.StringLiteral).Select(s => s.Span.ToString().Trim('"'));

        public static readonly TokenListParser<DMToken, string[]> Union =
            Identifier.AtLeastOnceDelimitedBy(Token.EqualTo(DMToken.Bar));

        public static readonly TokenListParser<DMToken, Dictionary<string, object>> Dictionary =
            from _ in Token.EqualTo(DMToken.ListKeyword)
            from __ in Token.EqualTo(DMToken.OpenParen)
            from pairs in Assignment.ManyDelimitedBy(Token.EqualTo(DMToken.Comma))
            from ___ in Token.EqualTo(DMToken.CloseParen)
            select pairs.ToDictionary(p => p.Key, p => p.Value);

        public static readonly TokenListParser<DMToken, object> Expression =
            from value in
                Integer.Select(i => i as object)
                .Or(Union.Select(u => u as object))
                .Or(String.Select(s => s as object))
                .Or(Dictionary.Select(d => d as object))
                .Or(Boolean.Select(d => d as object))
                .Or(Identifier.Select(r => r as object))
            select value;

        public static readonly TokenListParser<DMToken, KeyValuePair<string, object>> Assignment =
            from variableName in Identifier
            from _ in Token.EqualTo(DMToken.Equals)
            from value in Expression
            select new KeyValuePair<string, object>(variableName, value);

        public static readonly TokenListParser<DMToken, KeyValuePair<string, object>> LoneAssignment =
            from _ in Token.EqualTo(DMToken.LeadWhitespace)
            from ass in Assignment
            from __ in Token.EqualTo(DMToken.Eol)
            select ass;

        public static readonly TokenListParser<DMToken, dynamic> Object =
            from path in ReferencePath
            from _ in Token.EqualTo(DMToken.Eol)
            from values in LoneAssignment.Where(kvp => (kvp.Key ?? kvp.Value) != null)
                .Many().Select(a =>
                {
                    var eo = new ExpandoObject();
                    var eoColl = eo as IDictionary<string, object>;

                    eoColl["path"] = path;

                    foreach (var item in a)
                    {
                        eoColl.Add(item);
                    }
                    return eo as dynamic;
                })
            select values;

        public static readonly TokenListParser<DMToken, dynamic[]> ObjectList =
            (from obj in Object
             from _ in Token.EqualTo(DMToken.Eol).Many()
             select obj).Many().AtEnd();

        public static bool DynHas(dynamic dyn, string valName)
        {
            IDictionary<string, object> properties = dyn as IDictionary<string, object>;
            return properties.ContainsKey(valName);
        }
    }
}
