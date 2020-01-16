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

        public static readonly TokenListParser<DMToken, string> MemberAccess =
            from path in Token.EqualTo(DMToken.Identifier).AtLeastOnceDelimitedBy(Token.EqualTo(DMToken.Dot))
            select string.Join(".", path);

        public static readonly TokenListParser<DMToken, int> Integer =
             Token.EqualTo(DMToken.NumericLiteral)
                .Apply(Numerics.IntegerInt32);

        public static readonly TokenListParser<DMToken, bool> Boolean =
            Token.EqualTo(DMToken.BooleanLiteral).Select(x => bool.Parse(x.Span.ToString()));

        public static readonly TokenListParser<DMToken, string> String =
            Token.EqualTo(DMToken.StringLiteral).Select(s => s.Span.ToString().Trim('"'));

        public static readonly TokenListParser<DMToken, object[]> Invocation =
            from name in Token.EqualTo(DMToken.Identifier)
            from _ in Token.EqualTo(DMToken.OpenParen)
            from parameters in Expression.ManyDelimitedBy(Token.EqualTo(DMToken.Comma))
            from __ in Token.EqualTo(DMToken.CloseParen)
            select parameters.Prepend(name.Span.ToString()).ToArray();

        public static readonly TokenListParser<DMToken, Dictionary<string, object>> Dictionary =
            from _ in Token.EqualTo(DMToken.ListKeyword)
            from __ in Token.EqualTo(DMToken.OpenParen)
            from pairs in Assignment.ManyDelimitedBy(Token.EqualTo(DMToken.Comma))
            from ___ in Token.EqualTo(DMToken.CloseParen)
            select pairs.ToDictionary(p => p.Key, p => p.Value);

        public static readonly TokenListParser<DMToken, object> Expression =
            from value in
                Integer.Select(i => i as object)
                .Or(String.Select(s => s as object))
                .Or(Dictionary.Select(d => d as object))
                .Or(Boolean.Select(d => d as object))
                .Or(Invocation.Try().Select(i => i as object))
                .Or(MemberAccess.Select(ma => ma as object))
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

        // public static readonly TokenListParser<DMToken, object> Statement =
        //     Assignment.Select(a => a as object)
        //         .Or(Invocation.Select(i => i as object))
        //         .Or(ForLoop.Select(fl => fl));

        // public static readonly TokenListParser<DMToken, object> LoneStatement =
        //     from _ in Token.EqualTo(DMToken.LeadWhitespace)
        //     from value in Statement
        //     from ___ in Token.EqualTo(DMToken.Eol)
        //     select value;

        // public static TokenListParser<DMToken, (object value, int indent)> GetStatementIndent =
        //     from indent in Token.EqualTo(DMToken.LeadWhitespace).Select(ws => ws.Span.Length)
        //     from value in Statement
        //     from ___ in Token.EqualTo(DMToken.Eol)
        //     select (value, indent);

        // public static TokenListParser<DMToken, object> IndentedStatement(int indent) =>
        //     from _ in Token.EqualTo(DMToken.LeadWhitespace).Where(ws => ws.Span.Length >= indent)
        //     from value in Statement
        //     from ___ in Token.EqualTo(DMToken.Eol)
        //     select value;

        // public static readonly TokenListParser<DMToken, object> ForLoop =
        //     from _ in Token.EqualTo(DMToken.ForKeyword)
        //     from __ in Token.EqualTo(DMToken.OpenParen)
        //     from ass in Assignment
        //     from ___ in Token.EqualTo(DMToken.Comma)
        //     from expr in Expression
        //     from ____ in Token.EqualTo(DMToken.Comma)
        //     from state in Statement
        //     from _____ in Token.EqualTo(DMToken.CloseParen)
        //     from first in GetStatementIndent
        //     from body in IndentedStatement(first.indent).Many()
        //     select (ass, expr, state, body.Prepend(first)) as object;

        public static readonly TokenListParser<DMToken, string[]> ParameterDefinition =
            from _ in Token.EqualTo(DMToken.OpenParen)
            from values in Identifier.ManyDelimitedBy(Token.EqualTo(DMToken.Comma))
            from __ in Token.EqualTo(DMToken.CloseParen)
            select values;

        //public static readonly TokenListParser<DMToken, object> Function =
        //    from path in ReferencePath
        //    from _ in ParameterDefinition
        //    from __ in Token.EqualTo(DMToken.Eol)
        //    from ___ in LoneStatement.Many()
        //    from ____ in Token.EqualTo(DMToken.ReturnKeyword)
        //    select null as object;

        public static readonly TokenListParser<DMToken, ExpandoObject> Object =
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
                    return eo;
                })
            select values;

        public static readonly TokenListParser<DMToken, ExpandoObject[]> ObjectList =
            (from obj in Object//.Try().Or(Function.Select(x => (dynamic)x))
             from _ in Token.EqualTo(DMToken.Eol).Many()
             select obj).Many();

        public static bool DynHas(dynamic dyn, string valName)
        {
            IDictionary<string, object> properties = dyn as IDictionary<string, object>;
            return properties.ContainsKey(valName);
        }
    }
}
