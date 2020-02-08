using System;
using System.Collections.Generic;
using System.Text;

namespace husk
{
    public class Environment
    {
        public Dictionary<string, DeclType> typeDeclarations;
        public Dictionary<string, FunctionType> funcDeclarations;

        public Environment()
        {
            typeDeclarations = new Dictionary<string, DeclType>();
            funcDeclarations = new Dictionary<string, FunctionType>();
        }
    }
}
