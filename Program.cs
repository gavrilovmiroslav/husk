using System;
using System.Collections.Generic;
using System.IO;

namespace husk
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText("prelude.hsk");
            var tokenizer = new Tokenizer();

            Console.WriteLine("\n=========   LEXING  =========\n");
            if(tokenizer.Apply(text, out List<Token> tokens))
                foreach(var i in tokens)
                    Console.WriteLine(i);

            Console.WriteLine("\n=========  PARSING  =========\n");
            var parser = new Parser();
            if (parser.Apply(tokens, out Environment env))
                Console.WriteLine(env);
        }
    }
}
