using System;
using System.IO;

namespace husk
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new HuskParser();
            var interpreter = new HuskInterpreter();
            var code = File.ReadAllText("prelude.hsk").Replace(System.Environment.NewLine, ";");            
            
            var tokens = parser.Parse(code);
            foreach (var tok in tokens) Console.WriteLine(tok);
            //interpreter.Interpret(tokens);
/*
            parser.Parse("x: 't; y: 't");
            parser.Parse("m: 'f 'a");
            parser.Parse("unwrap: 'f 'a -> 'a");
            parser.Parse("faa: 'f (a -> a)");
            parser.Parse("fmap: (a -> b) -> 'f a -> 'f b");
            parser.Parse("let fmap f fa = ?");
            parser.Parse("decltype either a b");
            parser.Parse("decltype boolean");
            parser.Parse("true: boolean");
            parser.Parse("false: boolean");
            parser.Parse("left: 't -> 'u -> either 't 'u");
            parser.Parse("right: 't -> 'u -> either 't 'u");
            parser.Parse("from-left: a -> either a b -> a");
            parser.Parse("from-right: b -> either a b -> b");
*/

        }
    }
}
