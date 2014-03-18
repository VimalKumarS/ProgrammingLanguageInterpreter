using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS252Project
{
    public abstract class Statement
    {

    }
    public class DVar : Statement
    {
        public string Var;
        public Expr Expr;
    }
    public class Print : Statement
    {
        public Expr Expr;
    }
    public class Assign : Statement
    {
        public string Var;
        public Expr Expr;
    }
    public class Forlp : Statement
    {
        public string Var;
        public Expr From;
        public Expr To;
        public Statement stmt;
    }
    public class ReadInt : Statement
    {
        public string Ident;
    }
    public class Sequence : Statement
    {
        public Statement stmt1;
        public Statement stmt2;
    }
    public abstract class Expr { }
    public class Stringvalue : Expr
    {
        public string value;
    }
    public class IntFloatValue : Expr
    {
        public float value;
    }
    public class Variable : Expr
    {
        public string Ident;
    }
    public class BinExpr : Expr
    {
        public Expr left;
        public Expr right;
        public BinOp op;

    }

    [Flags]
    public enum BinOp
    {
        Add = 1,
        Sub = 2,
        Mult = 4,
        Div = 8,
    }
}
