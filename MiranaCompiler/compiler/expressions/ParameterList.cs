using System.Collections.Generic;
using System.Linq;

// this file contains non-expression non-statement syntax trees
namespace MiranaCompiler.Tree
{
    class ParameterList : SyntaxTree
    {
        public IFunctionDefinition DeclaringLiteralFunction { get; }
        private List<ParameterDefinition> Parameters { get; } = new();
        public ParameterList(IFunctionDefinition declaringLiteralFunction)
        {
            DeclaringLiteralFunction = declaringLiteralFunction;
        }
        public void AddParameter(string name, MiranaType? declaringType)
        {
            ParameterDefinition p = new(name, declaringType, DeclaringLiteralFunction);
            Parameters.Add(p);
        }
        public ParameterDefinition? FindParameter(string name)
        {
            return Parameters.LastOrDefault(t => t.Name == name);
        }

        public override string GetLiteralRepresentation()
        {
            return $"({Parameters.ConcatStringList()})";
        }
    }

    class PredicateExpression : SyntaxTree
    {
        public Expression? Exp { get; }
        public VarDeclare? VarDeclare { get; }
        public PredicateExpression(Expression exp)
        {
            Exp = exp;
        }
        public PredicateExpression(VarDeclare varDeclare)
        {
            VarDeclare = varDeclare;
        }

        public LocalVar? FindLocal(string name)
        {
            return VarDeclare?.Vars.LastOrDefault(t => t.Name == name);
        }
        public override string GetLiteralRepresentation()
        {
            return VarDeclare?.GetLiteralRepresentation()??Exp!.GetLiteralRepresentation();
        }
    }

    sealed class ExpressionList : SyntaxTree
    {
        public Expression[] Expressions { get; }
        public ExpressionList(IEnumerable<Expression> expressions)
        {
            Expressions = expressions.ToArray();
        }
        public override string GetLiteralRepresentation()
        {
            return Expressions.ConcatStringList();
        }
    }
}
