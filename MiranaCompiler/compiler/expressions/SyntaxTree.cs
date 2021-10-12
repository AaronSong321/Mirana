

using Antlr4.Runtime.Tree;

namespace MiranaCompiler.Tree
{
    record SourceLocation(int Line, int Column) 
    {
        public SourceLocation(ITerminalNode node) : this(node.Symbol.Line, node.Symbol.Column) { }
        
    }

    abstract class SyntaxTree
    {
        public SyntaxTree(SourceLocation? sourceLocation) {
            SourceLocation = sourceLocation;
        }
        public SyntaxTree() {
            
        }

        public SyntaxTree? Parent { get; set; }
        public SourceLocation? SourceLocation { get; set; }
        public abstract string GetLiteralRepresentation();
    }

}