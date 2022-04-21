using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MiranaCompiler.grammar;


namespace MiranaCompiler
{
    internal class CompileUnit
    {
        public string FilePath { get; }
        public string AllText { get; private set; }
        public CompileUnit(string file)
        {
            FilePath = file;
            AllText = File.ReadAllText(FilePath);
        }
        private readonly List<string> errorList = new();
        public void AddError(ParserRuleContext context, int code, string message)
        {
            AddError($"Error Mira{code} ({context.start.Line}, {context.start.Column}): {message}");
        }
        public void AddError(string s) => errorList.Add(s);
        public bool HasError() => errorList.Count != 0;
        public void PrintErrors() => errorList.ForEach(Console.WriteLine);
        public int ErrorNumber => errorList.Count;

        internal void Compile()
        {
            var preprocessor = new Preprocessor(this);
            string preprocessText = preprocessor.Process(AllText);
            if (HasError())
                return;
            AllText = preprocessText;
            var lexer = new miranaLexer(new AntlrInputStream(AllText));
            var parser = new miranaParser(new CommonTokenStream(lexer));
            parser.AddErrorListener(new MiranaErrorListener(this));
            var chunk = parser.chunk();
            if (HasError()) {
                return;
            }
            new MiranaToLuaWriter(this, chunk).Compile();
        }
    }
}
