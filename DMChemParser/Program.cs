using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DMChemParser
{
    class Program
    {
        static readonly Uri recipeUri = new Uri("https://raw.githubusercontent.com/tgstation/tgstation/master/code/modules/reagents/chemistry/recipes/medicine.dm");

        static async Task Main(string[] args)
        {
            var http = new HttpClient();
            var recipeDM = await http.GetStringAsync(recipeUri);

            var tokenizer = new DMTokenizer();
            foreach (var token in tokenizer.Tokenize(recipeDM))
            {
                Console.WriteLine($"{token.Kind.ToString().PadRight(20)} | {token.ToStringValue()}");
            }
        }
    }
}
