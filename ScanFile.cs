using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace CS252Project
{
        
    public class ScanFile
    {

        //Token used for parser
        IList<object> token;

        /// <summary>
        ///  Binary operation
        /// </summary>
        [Flags]
       public enum op
        {
            Add=1,
            Sub=2,
            Mult=4,
            Div=8,
            Equal=16,
            SemiColon=32,
            Colon=64
         }

        public IList<object> Token
        {
            get { return token; }
            set { token = value; }
        }

        public ScanFile()
        {
        }

        public ScanFile(TextReader readTxtfile)
        {
            this.token = new List<object>();
            this.Scan(readTxtfile);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readTxtfile"> Type of TextReader : </param>
        private void Scan(TextReader readTxtfile)
        {
            while (readTxtfile.Peek() != -1)
            {
                char ch = (char)readTxtfile.Peek();

                if (char.IsWhiteSpace(ch)) //remove space
                {
                    readTxtfile.Read();
                }
                else if (char.IsLetter(ch) || ch == '_') // reading letter / or reading input from console
                {
                    StringBuilder txtbuild = new StringBuilder();

                    while (char.IsLetter(ch) || ch == '_')
                    {
                        txtbuild.Append(ch);
                        readTxtfile.Read();
                        if (readTxtfile.Peek() == -1)
                        {
                            break;
                        }
                        else
                        {
                            ch = (char)readTxtfile.Peek();
                        }

                    }
                    this.token.Add(txtbuild);
                }
                else if (ch=='"') // checking for double quotes
                {
                    StringBuilder txtbuild = new StringBuilder();

                    readTxtfile.Read();
                    ch = (char)readTxtfile.Peek();
                    txtbuild.Append('"');
                    while (ch != '"')
                    {
                        txtbuild.Append(ch);
                        readTxtfile.Read();
                        ch = (char)readTxtfile.Peek();
                        
                    }
                    txtbuild.Append('"');
                    readTxtfile.Read();
                    this.token.Add(txtbuild);

                }
                else if (char.IsDigit(ch)) // check for character is number
                {
                    StringBuilder txtbuild = new StringBuilder();
                    while (char.IsDigit(ch) || ch=='.')
                    {
                        txtbuild.Append(ch);
                        readTxtfile.Read();

                        ch = (char)readTxtfile.Peek();

                    }
                    this.token.Add(float.Parse(txtbuild.ToString()));

                }
                else // checking for binary operator
                {
                    switch (ch)
                    {
                        case '+':
                            readTxtfile.Read();
                            this.token.Add(op.Add);
                            break;
                        case '-':
                            readTxtfile.Read();
                            this.token.Add(op.Sub);
                            break;
                        case '/':
                            readTxtfile.Read();
                            this.token.Add(op.Div);
                            break;
                        case ':':
                            readTxtfile.Read();
                            this.token.Add(op.Colon);
                            break;
                        case '*':
                            readTxtfile.Read();
                            this.token.Add(op.Mult);
                            break;
                        case '=':
                            readTxtfile.Read();
                            this.token.Add(op.Equal);
                            break;
                        case ';':
                            readTxtfile.Read();
                            this.token.Add(op.SemiColon);
                            break;
                        default:
                            break;

                    }
                }
            }
        }

    }
}
