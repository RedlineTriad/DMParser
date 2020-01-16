using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DMChem.Parser;

namespace DMChem
{
    class Program
    {
        static readonly Uri recipeUri = new Uri("https://raw.githubusercontent.com/tgstation/tgstation/master/code/modules/reagents/chemistry/recipes/medicine.dm");

        static async Task Main(string[] args)
        {
            var http = new HttpClient();
            var recipeDM = await http.GetStringAsync(recipeUri);

            var tokenizer = new DMTokenizer();
            var tokens = tokenizer.Tokenize(string.Join("\n", recipeDM.Split('\n').Take(202)));
            // foreach (var token in tokens)
            // {
            //     Console.WriteLine($"{token.Kind.ToString().PadRight(20)} | {token.ToStringValue()}");
            // }
            var parsed = DMParser.ObjectList(tokens);
            if (parsed.ErrorPosition.HasValue)
            {
                Console.WriteLine(parsed);
                var errorLine = parsed.ErrorPosition.Line;
                var lines = recipeDM.Split('\n');
                var countedLines = lines.Zip(Enumerable.Range(1, lines.Length + 1), (a, b) => $"{b} | {a}");
                Console.WriteLine(string.Join('\n', countedLines.Skip(errorLine - 2).Take(3)));
            }
            if (parsed.HasValue)
            {
                WriteReactions(parsed.Value);
            }
        }

        private static void WriteReactions(dynamic[] reactions)
        {
            var indent = 0;
            foreach (var reaction in reactions)
            {
                Print($"{reaction.path}:");
                indent++;

                Print($"name: {reaction.name}");

                Print("required_reagents:");
                indent++;
                foreach (var required in reaction.required_reagents)
                {
                    Print($"{required.Key}: {required.Value}");
                }
                indent--;

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
                    Print($"temperature:");
                    indent++;
                    if (DMParser.DynHas(reaction, "is_cold_recipe"))
                    {
                        Print($"min: {reaction.required_temp}");
                    }
                    else
                    {
                        Print($"max: {reaction.required_temp}");
                    }
                    indent--;
                }

                Print($"results:");
                indent++;
                foreach (var result in reaction.results)
                {
                    Print($"{result.Key}: {result.Value}");
                }
                indent--;

                indent--;
                Console.WriteLine();
            }

            void Print(string text){
                Console.WriteLine(new string(' ', indent * 2) + text);
            }
        }
    }
}
