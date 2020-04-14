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

            var uri = new Uri(args[0]);
            var dmText = await http.GetStringAsync(uri);
            dmText = RemoveFunctions(dmText);
            var tokens = tokenizer.Tokenize(dmText);
            var obj = DMParser.ObjectList(tokens);
            if (obj.ErrorPosition.HasValue)
            {
                Console.WriteLine(obj);
                //var errorLine = objs.ErrorPosition.Line;
                //WriteSurrounding(objs, errorLine, 3);
            }
            var yaml = serializer.Serialize(obj.Value);
            Console.WriteLine(yaml);
        }

        private static string RemoveFunctions(string recipeDM)
        {
            var regex = new Regex(@"(?:\/[\w_]+)+\(.*?\)(?:(?!\n\/datum).)*", RegexOptions.Singleline);
            return regex.Replace(recipeDM, "");
        }
    }
}
