using System;
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
            var tokens = tokenizer.Tokenize(recipeDM);
            // foreach (var token in tokens)
            // {
            //     Console.WriteLine($"{token.Kind.ToString().PadRight(20)} | {token.ToStringValue()}");
            // }
            var parsed = DMParser.Object(tokens);
            Console.WriteLine(parsed);
            Console.WriteLine(parsed.Value);
        }
    }
}
