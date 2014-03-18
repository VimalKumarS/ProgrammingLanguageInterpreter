using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS252Project
{
    public class ParserAST
    {
        private IList<object> iList;
        int initCount = 0; // keep track of number of token to be parsed
        private readonly Statement result;

        public ParserAST(IList<object> iList)
        {
            // TODO: Complete member initialization
            this.iList = iList;
            this.initCount = 0;
            this.result = this.parseStatement();
        }

        public Statement Result
        {
            get { return result; }
        }


        public Statement parseStatement()
        {
            Statement stmt = null;

            if (this.initCount == iList.Count)
            {
                // if token is empty
            }

            if (this.iList[this.initCount].ToString().Equals("print"))
            {
                this.initCount++;
                //string value = ((System.Text.StringBuilder)this.iList[this.initCount]).ToString();
                //Stringvalue stringLiteral = new Stringvalue();
                //stringLiteral.value = value;
                Print pobj = new Print();
                pobj.Expr = this.GetExpr();// stringLiteral;

               // this.initCount++;
                stmt = pobj;
            }
            else if (this.iList[this.initCount].ToString().Equals("var"))
            {
                DVar var = new DVar();
                this.initCount++;

                var.Var = this.iList[this.initCount].ToString();
                this.initCount++;
                if (this.iList[this.initCount].Equals(ScanFile.op.Equal))
                {
                    this.initCount++;
                    var.Expr = this.GetExpr();
                }
                stmt = var;
            }
            else if (this.iList[this.initCount].ToString().Equals("read_int"))
            {
                this.initCount++;
                ReadInt readInt = new ReadInt();

                if (!string.IsNullOrEmpty(this.iList[this.initCount].ToString()))
                {
                    readInt.Ident = this.iList[this.initCount++].ToString();
                    stmt = readInt;
                }

            }
            else if (this.iList[this.initCount].ToString().Equals("for"))
            {
                this.initCount++;
                Forlp forLoop = new Forlp();
                forLoop.Var = this.iList[this.initCount].ToString();
                this.initCount++;
                if (this.iList[this.initCount].Equals(ScanFile.op.Equal))
                {
                    this.initCount++;
                    forLoop.From = this.GetExpr();
                }
                if (this.iList[this.initCount].ToString().Equals("to"))
                {
                    this.initCount++;
                    forLoop.To = this.GetExpr();
                }
                if (this.iList[this.initCount].ToString().Equals("do"))
                {
                    this.initCount++;
                    forLoop.stmt = this.parseStatement();
                    this.initCount++; // for semicolon
                }
                stmt = forLoop;
                if (this.iList[this.initCount].ToString().Equals("end"))
                {
                    //this.initCount++;
                }
                //this.initCount++;
            }
            else if (!string.IsNullOrEmpty(this.iList[this.initCount].ToString()))
            {
                Assign assign = new Assign();
                assign.Var = this.iList[this.initCount++].ToString();

                this.initCount++; // to expect = sign
               // assign.Expr = this.GetExpr();
                assign.Expr = GetBinop(this.GetExpr());
                

                stmt = assign;
            }


            if (this.initCount < this.iList.Count && this.iList[this.initCount].Equals(ScanFile.op.SemiColon))
            {
                this.initCount++;
                if (this.initCount < this.iList.Count && !this.iList[this.initCount].ToString().Equals("end"))
                {
                    Sequence seq = new Sequence();
                    seq.stmt1 = stmt;
                    seq.stmt2 = this.parseStatement();
                    stmt = seq;
                }

            }
            return stmt;

        }

        private Expr GetBinop(Expr exp1)
        {
            Expr expr=null;
            BinExpr bexpr = new BinExpr();
            bexpr.left = exp1;
            //this.initCount++;
            
            switch ((CS252Project.ScanFile.op)(this.iList[this.initCount]))
            {
                case CS252Project.ScanFile.op.Add:
                    bexpr.op = BinOp.Add;
                    this.initCount++;
                    break;
                case CS252Project.ScanFile.op.Sub:
                    bexpr.op = BinOp.Sub;
                    this.initCount++;
                    break;
                case CS252Project.ScanFile.op.Mult:
                    bexpr.op = BinOp.Mult;
                    this.initCount++;
                    break;
                case CS252Project.ScanFile.op.Div:
                    bexpr.op = BinOp.Div;
                    this.initCount++;
                    break;
                default:
                    break;
            }
           // bexpr.op = BinOp.Sub;
            bexpr.right = this.GetExpr();
            if (!this.iList[this.initCount].Equals(ScanFile.op.SemiColon))
            {
                expr = GetBinop(bexpr);
            }
            else
            {
                if (bexpr.right == null) // if single assignment 
                {
                    return bexpr.left;
                }
                return bexpr;
            }

            return expr;
        }

        private Expr GetExpr()
        {

            if (this.iList[this.initCount] is System.Text.StringBuilder && this.iList[this.initCount].ToString().StartsWith("\""))
            {
                string value = ((System.Text.StringBuilder)this.iList[this.initCount++]).ToString();
                Stringvalue stringLiteral = new Stringvalue();
                stringLiteral.value = value.Replace("\"","");
                return stringLiteral;
            }
            else if (this.iList[this.initCount] is float)
            {
                float intValue = (float)this.iList[this.initCount++];
                IntFloatValue intLiteral = new IntFloatValue();
                intLiteral.value = intValue;
                return intLiteral;
            }
            else if (this.iList[this.initCount] is StringBuilder)
            {
                string ident = this.iList[this.initCount++].ToString();   // considering that is a variable
                Variable var = new Variable();
                var.Ident = ident;
                return var;
            }

            else
            {
                // to throw exception
                return null;
            }
        }

        public BinExpr bexpr { get; set; }
    }
}
