using System;
using System.IO;

namespace husk
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new HuskParser();
            var interpreter = new HuskEvaluator();            
            var tokens = parser.Parse(File.ReadAllText("prelude.hsk"));
            if (interpreter.Evaluate(tokens))
                Console.WriteLine(interpreter.Interpret());
        }
    }
}
