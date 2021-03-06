
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



using Antlr4;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AngouriMath.Core.FromString
{
    class AngouriMathTokenStream : CommonTokenStream
    {
        public AngouriMathTokenStream(ITokenSource source)
            : base(source) { }
    }

    static class Parser
    {
        static readonly Dictionary<string, int> antlrDict =
            new Dictionary<string, int>();
        static Parser()
        {
            var tokenReader = new StreamReader(typeof(Parser).Assembly.GetManifestResourceStream("AngouriMath.Core.FromString.Antlr.Angourimath.tokens"));
            while (!tokenReader.EndOfStream)
            {
                var t = tokenReader.ReadLine().Split('=');
                antlrDict.Add(t[0], int.Parse(t[1], System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        static public Entity Parse(string source)
        {
            AngouriMathTokenStream GetTokenStream(string source)
            {
                var stream = new AntlrInputStream(source);
                var lexer = new AngourimathLexer(stream);

                return new AngouriMathTokenStream(lexer);
            }

            var tokenStream = GetTokenStream(source);
            tokenStream.Fill();
            var tokenList = tokenStream.GetTokens();
            if (tokenList.Count == 0)
                throw new ParseException("input string is invalid");
            source = Parser.PreProcess(tokenList);
            var tokens = GetTokenStream(source);

            var parser = new AngourimathParser(tokens);
            parser.Parse();
            var result = Parser.PostProcess(parser.Result);
            return result;
        }

        static string PreProcess(IList<IToken> tokens)
        {
            int NUMBER = antlrDict[nameof(NUMBER)];
            int ID = antlrDict[nameof(ID)];
            int PARENTHESIS_OPEN = antlrDict["'('"];
            int PARENTHESIS_CLOSE = antlrDict["')'"];
            int MULTIPLY = antlrDict["'*'"];
            int POWER = antlrDict["'^'"];

            const int FUNCTION = -0xFF;
            const int VARIABLE = -0xEE;

            tokens = tokens.Where(token => token.Channel == 0).ToList();

            bool IsTypeEqual(IToken token, int type)
            {
                if (token.Type == ID)
                {
                    if (SyntaxInfo.goodStringsForFunctions.ContainsKey(token.Text))
                        return type == FUNCTION;
                    else
                        return type == VARIABLE;
                }
                else return token.Type == type;
            }

            /// <summary>
            /// Provided two types of tokens, returns position of first token if
            /// the pair if found, -1 otherwisely.
            /// </summary> 

            int FindSubPair(int type1, int type2)
            {
                for (int i = 0; i < tokens.Count - 1; i++)
                    if (IsTypeEqual(tokens[i], type1) && IsTypeEqual(tokens[i + 1], type2))
                        return i;
                return -1;
            }

            /// <summary>
            /// Finds all occurances of [t1, t2] and inserts token in between each of them
            /// </summary>
            void InsertIntoPair(int type1, int type2, IToken token)
            {
                int pos;
                while ((pos = FindSubPair(type1, type2)) != -1)
                {
                    tokens.Insert(pos + 1 /* we need to keep the first one behind*/, token);
                }
            }

            IToken multiplyer = new CommonToken(MULTIPLY, "*");
            IToken power = new CommonToken(POWER, "^");

            // 2x -> 2 * x
            InsertIntoPair(NUMBER, VARIABLE, multiplyer);

            // x y -> x * y
            InsertIntoPair(VARIABLE, VARIABLE, multiplyer);

            // 2( -> 2 * (
            InsertIntoPair(NUMBER, PARENTHESIS_OPEN, multiplyer);

            // )2 -> ) ^ 2
            InsertIntoPair(PARENTHESIS_CLOSE, NUMBER, power);

            // x( -> x * (
            InsertIntoPair(VARIABLE, PARENTHESIS_OPEN, multiplyer);

            // )x -> ) * x
            InsertIntoPair(PARENTHESIS_CLOSE, VARIABLE, multiplyer);

            // x2 -> x ^ 2
            InsertIntoPair(VARIABLE, NUMBER, power);

            // 3 2 -> 3 ^ 2
            InsertIntoPair(NUMBER, NUMBER, power);

            // 2sqrt -> 2 * sqrt
            InsertIntoPair(NUMBER, FUNCTION, multiplyer);

            // x sqrt -> x * sqrt
            InsertIntoPair(VARIABLE, FUNCTION, multiplyer);

            // )sqrt -> ) * sqrt
            InsertIntoPair(PARENTHESIS_CLOSE, FUNCTION, multiplyer);

            // )( -> ) * (
            // )sqrt -> ) * sqrt
            InsertIntoPair(PARENTHESIS_CLOSE, PARENTHESIS_OPEN, multiplyer);

            var builder = new StringBuilder();
            tokens.RemoveAt(tokens.Count - 1); // remove <EOF> token
            foreach (var token in tokens)
            {
                builder.Append(token.Text);
            }
            return builder.ToString();
        }

        static Entity PostProcess(Entity result)
        {
            result = result.Substitute("i", MathS.i);
            result = SynonymFunctions.Synonymize(result);
            return result;
        }
    }
}