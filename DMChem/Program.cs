using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DMChem.Parser;
using YamlDotNet;
using YamlDotNet.Serialization;
using Superpower.Model;
using System.Collections.Generic;
using System.IO;

namespace DMChem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var lines = await File.ReadAllLinesAsync(args[0]);
            var http = new HttpClient();
            var serializer = new SerializerBuilder()
                .DisableAliases()
                .WithTypeConverter(new ColorYamlTypeConverter())
                .Build();
            var tokenizer = new DMTokenizer();
            DMParser.Defines.Add("REAGENTS_METABOLISM", 0.4m);
            DMParser.Defines.Add("REM", 0.1m);
            DMParser.Defines.Add("T0C", 273.15m);
            DMParser.Defines.Add("BLOOD_VOLUME_NORMAL", 560m);

            await Task.WhenAll(lines.Skip(1)
                .Where(l => !l.StartsWith("#"))
                .Select(r => new Uri(lines[0] + r))
                .Select(async uri => await http.GetStringAsync(uri))
                .Select(async dmText => RemoveFunctions(await dmText))
                .Select(async dmText => tokenizer.Tokenize(await dmText))
                // .Select(async tokens =>
                // {
                //     foreach(var token in await tokens) {
                //         Console.WriteLine($"{token.Position.Line}:{token.Position.Column} | {token.Kind.ToString().PadRight(15)} | {token.ToStringValue()}");
                //     }
                //     return await tokens;
                // })
                .Select(async tokens => DMParser.ObjectList(await tokens))
                .Select(async obj =>
                {
                    var objs = await obj;
                    if (objs.ErrorPosition.HasValue)
                    {
                        Console.WriteLine(objs);
                        //var errorLine = objs.ErrorPosition.Line;
                        //WriteSurrounding(objs, errorLine, 3);
                    }
                    return objs;
                })
                .Select(async objs => serializer.Serialize((await objs).Value))
                .Select(async yaml => Console.WriteLine(await yaml))
                ).ConfigureAwait(false);
        }

        private static string RemoveFunctions(string recipeDM)
        {
            var regex = new Regex(@"(?:\/[\w_]+)+\(.*?\)(?:(?!\n\/datum).)*", RegexOptions.Singleline);
            return regex.Replace(recipeDM, "");
        }
    }
}
