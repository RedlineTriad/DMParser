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

namespace DMChem
{
    class Program
    {
        static readonly Uri baseURI = new Uri("https://raw.githubusercontent.com/tgstation/tgstation/master/code/modules/reagents/chemistry/reagents/");
        static readonly string[] recipes = {
            "alcohol_reagents.dm",
            //"cat2_medicine_reagents.dm",
            //"drink_reagents.dm",
            //"drug_reagents.dm",
            //"food_reagents.dm",
            //"medicine_reagents.dm",
            //"other_reagents.dm",
            //"pyrotechnic_reagents.dm",
            //"toxin_reagents.dm",
            };

        static async Task Main(string[] args)
        {
            var recipeUri = baseURI + recipes[0];
            var http = new HttpClient();
            var serializer = new SerializerBuilder().Build();
            var tokenizer = new DMTokenizer();

            await Task.WhenAll(recipes
                .Select(r => new Uri(baseURI + r))
                .Select(async uri => await http.GetStringAsync(uri))
                .Select(async dmText => RemoveFunctions(await dmText))
                .Select(async dmText => tokenizer.Tokenize(await dmText))
                .Select(async tokens =>
                {
                    foreach(var token in await tokens) {
                        Console.WriteLine($"{token.Position.Line} | {token.Kind.ToString().PadRight(15)} | {token.ToStringValue()}");
                    }
                    return await tokens;
                })
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

        private static void WriteSurrounding(string text, int line, int range)
        {
            var lines = text.Split('\n');
            var countedLines = lines.Zip(Enumerable.Range(1, lines.Length + 1), (a, b) => $"{b} | {a}");
            Console.WriteLine(string.Join('\n', countedLines.Skip(line - range).Take(range * 2 - 1)));
        }
    }
}
