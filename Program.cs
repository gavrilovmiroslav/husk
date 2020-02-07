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

            if(tokenizer.Apply(text, out List<Token> result))
                foreach(var i in result)
                    Console.WriteLine(i);
        }
    }
}
