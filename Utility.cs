using System;
using System.Collections.Generic;
using System.Text;

namespace husk
{
    public static class ConsoleUtilities
    {
        private static Stack<ConsoleColor> colors = new Stack<ConsoleColor>();

        public static void PushColor(ConsoleColor color)
        {
            colors.Push(Console.ForegroundColor);
            Console.ForegroundColor = color;
        }

        public static void PopColor()
        {
            if(colors.Count > 0)
                Console.ForegroundColor = colors.Pop();
        }
    }
}
