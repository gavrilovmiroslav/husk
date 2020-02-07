using System;
using System.Collections.Generic;
using System.Text;

namespace husk
{
    public interface AST { }

    public interface TypeDef { }

    public class SimpleType : TypeDef
    {
        public string name;
        public string[] args;

        public SimpleType() { }

        public override string ToString()
        {
            string strArgs = "";
            if (args != null) strArgs = string.Join(' ', args);
            return $"{name} {strArgs}";
        }
    }

    public class FunctionType : TypeDef
    {
        public TypeDef[] types;

        public FunctionType() { }
    }

    public class DataDef
    {
        public TypeDef type;
        public string name;
        public string[] args;
    }

    public class DeclType : AST
    {
        public TypeDef declaredType;
        public List<DataDef> constructors;

        public DeclType() { }
    }
}
