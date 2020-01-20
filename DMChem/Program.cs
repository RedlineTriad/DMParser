using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DMChem.Parser;

namespace DMChem
{
    class Program
    {
        static readonly Uri recipeUri = new Uri("https://raw.githubusercontent.com/tgstation/tgstation/master/code/modules/reagents/chemistry/recipes/pyrotechnics.dm");

        static async Task Main(string[] args)
        {
            var http = new HttpClient();
            var recipeDM = await http.GetStringAsync(recipeUri);
            var regex = new Regex(@"(?:\/[\w_]+)+\(.*?\).+?(?=\n\/datum)", RegexOptions.Singleline);
            recipeDM = regex.Replace(recipeDM, "");

            var tokenizer = new DMTokenizer();
            var tokens = tokenizer.Tokenize(recipeDM);
            var parsed = DMParser.ObjectList(tokens);
            if (parsed.ErrorPosition.HasValue)
            {
                Console.WriteLine(parsed);
                var errorLine = parsed.ErrorPosition.Line;
                WriteSurrounding(recipeDM, errorLine, 3);
            }
            if (parsed.HasValue)
            {
                WriteReactions(parsed.Value);
            }
        }

        private static void WriteSurrounding(string text, int line, int range)
        {
            var lines = text.Split('\n');
            var countedLines = lines.Zip(Enumerable.Range(1, lines.Length + 1), (a, b) => $"{b} | {a}");
            Console.WriteLine(string.Join('\n', countedLines.Skip(line - range).Take(range * 2 - 1)));
        }

        private static void WriteReactions(dynamic[] reactions)
        {
            var indent = 0;
            foreach (var reaction in reactions)
            {
                Print($"{reaction.path}:");
                indent++;

                if (DMParser.DynHas(reaction, "name"))
                {
                    Print($"name: {reaction.name}");
                }

                if (DMParser.DynHas(reaction, "required_reagents"))
                {
                    Print("required_reagents:");
                    indent++;
                    foreach (var required in reaction.required_reagents)
                    {
                        Print($"{required.Key}: {required.Value}");
                    }
                    indent--;
                }

                if (DMParser.DynHas(reaction, "required_catalysts"))
                {
                    Print("required_catalysts:");
                    indent++;
                    foreach (var required in reaction.required_catalysts)
                    {
                        Print($"{required.Key}: {required.Value}");
                    }
                    indent--;
                }

                if (DMParser.DynHas(reaction, "required_temp"))
                {
                    Print("temperature:");
                    indent++;
                    var cold = DMParser.DynHas(reaction, "is_cold_recipe");
                    Print($"{(cold ? "min" : "max")}: {reaction.required_temp}");
                    indent--;
                }

                if (DMParser.DynHas(reaction, "results"))
                {
                    Print("results:");
                    indent++;
                    foreach (var result in reaction.results)
                    {
                        Print($"{result.Key}: {result.Value}");
                    }
                    indent--;
                }

                indent--;
                Console.WriteLine();
            }

            void Print(string text)
            {
                Console.WriteLine(new string(' ', indent * 2) + text);
            }
        }
    }
}
