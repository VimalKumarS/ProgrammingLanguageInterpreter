using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection.Emit;
using System.Reflection;

namespace CS252Project
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Input file not found!!!");

                return;
            }
            try
            {
                ScanFile sfile;
                FileInfo info = new FileInfo(args[0]);
                if (info.Extension.ToLower() != ".vml")
                {
                    Console.WriteLine("Invalid file type");
                    return;
                }
                using (TextReader readTxtfile = File.OpenText(args[0])) //  \\args[0] \\@"F:\SJSU\CS252\Project\CompilerWriting\samples\binaryop.vml"
                {
                    sfile = new ScanFile(readTxtfile);
                }
                ParserAST parser = new ParserAST(sfile.Token);
                Statement stmt = parser.Result;

                AssemblyName assmblyName = new AssemblyName(info.Name) ;//"Testmodule");
                AssemblyBuilder asmblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assmblyName, AssemblyBuilderAccess.Save);
                ModuleBuilder moduleBuildr = asmblyBuilder.DefineDynamicModule(info.Name + ".exe");
                TypeBuilder typBuilder = moduleBuildr.DefineType("VML");
                MethodBuilder methBuilder = typBuilder.DefineMethod("Main", MethodAttributes.Static, typeof(void), Type.EmptyTypes);

                codeGenerator = methBuilder.GetILGenerator();

                CallingStatement(stmt);
                codeGenerator.Emit(OpCodes.Ret);
                typBuilder.CreateType();
                moduleBuildr.CreateGlobalFunctions();
                asmblyBuilder.SetEntryPoint(methBuilder);

                asmblyBuilder.Save(info.Name + ".exe");
                Console.WriteLine(info.Name + ".exe" + " Code file generated.");

            }
            catch (Exception exp)
            {
                Console.Write(exp.Message);
                Console.WriteLine(exp.StackTrace);
            }
        }

        static Dictionary<string, LocalBuilder> localBuilder = new Dictionary<string, LocalBuilder>(); // like store in While interpreter
        static ILGenerator codeGenerator = null;

        /// <summary>
        /// Emit.OpCodes.Call -- Calls the method indicated by the passed method descriptor.
        /// Stloc -Pops the current value from the top of the evaluation stack and stores it in a the local variable list at a specified index.
        /// Ldstr -Pushes a new object reference to a string literal stored in the metadata.
        ///  Ldc_I4 -Pushes a supplied value of type int32 onto the evaluation stack as an int32.
        ///  Ldc_R4- Pushes a supplied value of type float32 onto the evaluation stack as type F 
        ///  Box -Converts a value type to an object reference 
        /// </summary>
        /// <param name="stmt"></param>
        public static void CallingStatement(Statement stmt)
        {
            if (stmt is Sequence)
            {
                Sequence seq = (Sequence)stmt;
                CallingStatement(seq.stmt1);
                CallingStatement(seq.stmt2);
            }
            else if (stmt is DVar)
            {
                DVar dvar = (DVar)stmt;
                localBuilder[dvar.Var] = codeGenerator.DeclareLocal(ExpressionType(dvar.Expr)); // declare the new variable 

                Assign assign = new Assign();
                assign.Var = dvar.Var;
                assign.Expr = dvar.Expr;
                CallingStatement(assign);
            }
            else if (stmt is Print)
            {
                System.Type t = ExpressionType(((Print)stmt).Expr);
                //http://dgtuts.weebly.com/5/post/2013/01/how-to-create-a-programming-language-using-c.html
                switch (((Print)stmt).Expr.ToString().Substring(((Print)stmt).Expr.ToString().LastIndexOf(".") + 1))
                {
                    case "Stringvalue":
                        codeGenerator.Emit(OpCodes.Ldstr, ((Stringvalue)((Print)stmt).Expr).value);
                        break;
                    case "IntFloatValue":
                        codeGenerator.Emit(OpCodes.Ldc_R4, ((IntFloatValue)((Print)stmt).Expr).value);

                        break;
                    case "Variable":
                        string ident = ((Variable)((Print)stmt).Expr).Ident;
                        Type deliveredType = ExpressionType(((Print)stmt).Expr);
                        codeGenerator.Emit(OpCodes.Ldloc, localBuilder[((Variable)((Print)stmt).Expr).Ident]);
                        codeGenerator.Emit(OpCodes.Box, typeof(float));
                        codeGenerator.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
                        break;
                    default:
                        break;
                }


                //Console.WriteLine("");
                //GenExpr(((Print)stmt).Expr, typeof(string));
                codeGenerator.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new System.Type[] { typeof(string) }));
            }
            else if (stmt is Assign)
            {
                Assign assign = (Assign)stmt;
                System.Type t = ExpressionType(((Assign)stmt).Expr);
                switch (((Assign)stmt).Expr.ToString().Substring(((Assign)stmt).Expr.ToString().LastIndexOf(".") + 1))
                {
                    case "Stringvalue":
                        codeGenerator.Emit(OpCodes.Ldstr, ((Stringvalue)((Assign)stmt).Expr).value);
                        break;
                    case "IntFloatValue":
                        codeGenerator.Emit(OpCodes.Ldc_R4, ((IntFloatValue)((Assign)stmt).Expr).value);

                        break;
                    case "Variable":
                        string ident = ((Variable)((Assign)stmt).Expr).Ident;
                        Type deliveredType = ExpressionType(((Assign)stmt).Expr);
                        codeGenerator.Emit(OpCodes.Ldloc, localBuilder[((Variable)((Assign)stmt).Expr).Ident]);
                        break;
                    case "BinExpr":
                        Dictionary<int, object> binaryOperation = new Dictionary<int, object>();
                        Expr leftexp = ((BinExpr)((Assign)stmt).Expr).left;
                        BinOp operation = ((BinExpr)((Assign)stmt).Expr).op;
                        Expr rightexp = ((BinExpr)((Assign)stmt).Expr).right;
                        //bool isBinExpflag = true;
                        int icount = 0;
                        while (leftexp is BinExpr)
                        {
                            binaryOperation.Add(icount++, rightexp);
                            binaryOperation.Add(icount++, operation);

                            rightexp = ((BinExpr)leftexp).right;

                            operation = ((BinExpr)leftexp).op;
                            leftexp = ((BinExpr)leftexp).left;


                        }
                        binaryOperation.Add(icount++, rightexp);
                        binaryOperation.Add(icount++, operation);
                        binaryOperation.Add(icount, leftexp);
                        codeGenerator.Emit(OpCodes.Ldc_R4, ((IntFloatValue)binaryOperation[icount]).value);
                        icount = icount - 1;
                        while (icount >= 0)
                        {
                            //int rightPos = icount;
                            int operationPos = icount;
                            int leftOperation = icount - 1;
                            icount = icount - 2;
                            codeGenerator.Emit(OpCodes.Ldc_R4, ((IntFloatValue)binaryOperation[leftOperation]).value);
                            BinaryOperationMapping(((BinOp)binaryOperation[operationPos]));
                        }

                        codeGenerator.Emit(OpCodes.Stloc, localBuilder[assign.Var]);
                        break;
                    default:
                        break;
                }
                /*
                 *  this.GenExpr(assign.Expr, this.TypeOfExpr(assign.Expr));
	        
                 */
                // LocalBuilder locb = localBuilder[assign.Var];

                //Pops the current value from the top of the evaluation stack and stores 
                //it in a the local variable list at a specified index.
                //codeGenerator.Emit(OpCodes.Stloc, localBuilder[assign.Var]);
                // GenExpr(assign.Expr, ExpressionType(assign.Expr));
                Store(assign.Var, ExpressionType(assign.Expr));
            }
            else if (stmt is ReadInt)
            {
                codeGenerator.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { }, null));
                codeGenerator.Emit(OpCodes.Call, typeof(float).GetMethod("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(string) }, null));
                Store(((ReadInt)stmt).Ident, typeof(float));
            }
            else if (stmt is Forlp)
            {
                Forlp forLoop = (Forlp)stmt;
                Assign assign = new Assign();
                assign.Var = forLoop.Var;
                assign.Expr = forLoop.From;
                CallingStatement(assign);

                Label testCond = codeGenerator.DefineLabel();
                codeGenerator.Emit(OpCodes.Br, testCond);
                Label body = codeGenerator.DefineLabel();
                codeGenerator.MarkLabel(body);
                CallingStatement(forLoop.stmt);

                codeGenerator.Emit(OpCodes.Ldloc, localBuilder[forLoop.Var]);
                codeGenerator.Emit(OpCodes.Ldc_R4, 1.0f);
                codeGenerator.Emit(OpCodes.Add);
                Store(forLoop.Var, typeof(float));
                codeGenerator.MarkLabel(testCond);
                codeGenerator.Emit(OpCodes.Ldloc, localBuilder[forLoop.Var]); //Loads the local variable at a specific index onto the evaluation stack.

                codeGenerator.Emit(OpCodes.Ldc_R4, ((IntFloatValue)(forLoop.To)).value);
               //  GenExpr(forLoop.To, typeof(float));
                codeGenerator.Emit(OpCodes.Blt, body);
            }
            // else if( stmt is
        }

        /// <summary>
        /// Emit the binary operation to be performed
        /// </summary>
        /// <param name="op"></param>
        private static void BinaryOperationMapping(BinOp op)
        {
            switch (op)
            {
                case BinOp.Add:
                    codeGenerator.Emit(OpCodes.Add);
                    break;
                case BinOp.Div:
                    codeGenerator.Emit(OpCodes.Div);
                    break;
                case BinOp.Mult:
                    codeGenerator.Emit(OpCodes.Mul);
                    break;
                case BinOp.Sub:
                    codeGenerator.Emit(OpCodes.Sub);
                    break;
            }
        }
        private static void Store(string name, System.Type type)
        {
            if (localBuilder.ContainsKey(name))
            {
                LocalBuilder locb = localBuilder[name];

                if (locb.LocalType == type)
                {
                    codeGenerator.Emit(OpCodes.Stloc, localBuilder[name]);
                }

            }

        }
        private static System.Type ExpressionType(Expr expression)
        {
            if (expression is Stringvalue)
            {
                return typeof(String);
            }
            else if (expression is IntFloatValue)
            {
                return typeof(float);
            }
            else if (expression is Variable)
            {
                Variable variable = (Variable)expression; // read the variable
                LocalBuilder ltype = localBuilder[variable.Ident];
                return ltype.LocalType; // return type from the store
            }
            else if (expression is BinExpr)
            {

            }
            return null;
        }
    }
}


//