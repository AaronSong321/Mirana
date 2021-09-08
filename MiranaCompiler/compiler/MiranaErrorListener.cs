using Antlr4.Runtime;

namespace MiranaCompiler
{
    internal class MiranaErrorListener : BaseErrorListener
    {
        private readonly CompileUnit compileUnit;
        public MiranaErrorListener(CompileUnit compileUnit)
        {
            this.compileUnit = compileUnit;
        }
        
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            compileUnit.AddError($"Syntax Error at ({line}, {charPositionInLine}): {msg}");
        }
    }
}
