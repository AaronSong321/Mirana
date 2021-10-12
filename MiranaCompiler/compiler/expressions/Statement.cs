
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace MiranaCompiler.Tree
{
    abstract class Statement : SyntaxTree
    {
        
    }

    class AssignState : Statement
    {
        public List<Expression> LeftExpressions { get; } = new();
        public List<Expression> RightExpressions { get; } = new();
        public AssignState()
        {
        }

        public override string GetLiteralRepresentation()
        {
            return $"{LeftExpressions.ConcatStringList()} = {RightExpressions.ConcatStringList()}";
        }
        public void AddLeftExpression(Expression e)
        {
            LeftExpressions.Add(e);
            e.Parent = this;
        }
        public void AddRightExpressions(Expression e)
        {
            RightExpressions.Add(e);
            e.Parent = this;
        }
    }

    class Block : Statement, IVarBlock
    {
        public List<Statement> NonFinalStatements { get; } = new();
        public ReturnStatement? ReturnStatement { get; set; }
        
        public void AddStatement(Statement statement)
        {
            statement.Parent = this;
            NonFinalStatements.Add(statement);
        }
        public override string GetLiteralRepresentation()
        {
            return string.Concat(NonFinalStatements.Select(t => t.GetLiteralRepresentation()), "\n");
        }        
        public IEnumerable<Statement> GetStatements()
        {
            foreach (var statement in NonFinalStatements) {
                yield return statement;
            }
            if (ReturnStatement is not null)
                yield return ReturnStatement;
        }

        private readonly List<LocalVar> locals = new();
        public void AddLocal(LocalVar variable)
        {
            locals.Add(variable);
        }
        public LocalVar? FindLocal(string name)
        {
            return locals.LastOrDefault(t => t.Name == name);
        }
    }

    class Chunk : Block
    {
        public Chunk():base()
        {
        }
    }

    enum CompoundAssignOperatorName
    {
        Add,
        Sub,
        Mul,
        Div,
        FloorDiv,
        Shl,
        Shr,
    }

    static partial class OperatorExtensions
    {
        public static string GetLiteralRepresentation(this CompoundAssignOperatorName compoundAssignOperatorName)
        {
            return compoundAssignOperatorName switch {
                CompoundAssignOperatorName.Add => "+",
                CompoundAssignOperatorName.Sub => "-",
                CompoundAssignOperatorName.Mul => "*",
                CompoundAssignOperatorName.Div => "/",
                CompoundAssignOperatorName.FloorDiv => "//",
                CompoundAssignOperatorName.Shl => "<<",
                CompoundAssignOperatorName.Shr => ">>",
                _ => throw new()
            } + "=";
        }
    }

    class CompoundAssignStatement : Statement
    {
        public Expression Left { get; }
        public Expression Right { get; }
        public CompoundAssignOperatorName CompoundAssignOperator { get; }
        
        public CompoundAssignStatement(Expression left, CompoundAssignOperatorName compoundAssignOperator, Expression right)
        {
            Left = left;
            left.Parent = this;
            Right = right;
            right.Parent = this;
            CompoundAssignOperator = compoundAssignOperator;
        }

        public override string GetLiteralRepresentation() => $"{Left.GetLiteralRepresentation()} {CompoundAssignOperator.GetLiteralRepresentation()} {Right.GetLiteralRepresentation()}";
    }

    sealed class ReturnStatement : Statement
    {
        public ExpressionList? ReturnedExps { get; }
        public ReturnStatement() { }
        public ReturnStatement(IEnumerable<Expression> expressions)
        {
            ReturnedExps = new(expressions);
        }
        public override string GetLiteralRepresentation()
        {
            return $"return{(ReturnedExps is not null ? " " + ReturnedExps.GetLiteralRepresentation() : "")}";
        }
    }

    class LabelStatement : Statement
    {
        public string Label { get; }
        public Statement LabelBefore { get; set; } = null!;

        public LabelStatement(string label)
        {
            Label = label;
        }
        public override string GetLiteralRepresentation()
        {
            return $"::{Label}::";
        }
    }

    class BreakStatement : Statement
    {
        public Statement BreakTo { get; set; } = null!;
        public override string GetLiteralRepresentation()
        {
            return "\nbreak";
        }
    }

    abstract class LoopStatement : Statement, IVarBlock
    {
        public Block LoopBlock { get; }
        public LoopStatement()
        {
            LoopBlock = new();
            LoopBlock.Parent = this;
        }
        public ReturnStatement? ReturnStatement {
            get => LoopBlock.ReturnStatement;
            set => LoopBlock.ReturnStatement = value;
        }
        public abstract LocalVar? FindLocal(string name);
        public void AddLocal(LocalVar variable)
        {
            LoopBlock.AddLocal(variable);
        }
        public void AddStatement(Statement statement)
        {
            LoopBlock.AddStatement(statement);
        }
        public IEnumerable<Statement> GetStatements()
        {
            return LoopBlock.GetStatements();
        }
    }

    class GotoStatement : Statement
    {
        public LabelStatement Label { get; }
        public GotoStatement(LabelStatement label)
        {
            Label = label;
        }

        public override string GetLiteralRepresentation()
        {
            return $"goto {Label.Label}";
        }
    }

    class WhileStatement : LoopStatement
    {
        public PredicateExpression Predicate { get; }
        public WhileStatement(PredicateExpression predicate)
        {
            Predicate = predicate;
            predicate.Parent = this;
        }

        public override LocalVar? FindLocal(string name)
        {
            return LoopBlock.FindLocal(name)??Predicate.FindLocal(name);
        }
        public override string GetLiteralRepresentation()
        {
            return $"while {Predicate.GetLiteralRepresentation()} do\n{LoopBlock.GetLiteralRepresentation()}\nend";
        }
    }

    // class IfStatement : Statement
    // {
    //     public class IfBranch : Block
    //     {
    //         public 
    //     }
    //     private Expression predicate;
    //     public Expression Predicate {
    //         get => predicate;
    //         set {
    //             predicate = value;
    //             predicate.Parent = this;
    //         }
    //     }
    // }

    class ForIStatement : LoopStatement
    {
        public LocalVar ControlVar { get; }
        public Expression ControlVarStart { get; }
        public Expression ControlVarEnd { get; }

        public ForIStatement(LocalVar controlVar, Expression controlVarStart, Expression controlVarEnd)
        {
            ControlVar = controlVar;
            ControlVarStart = controlVarStart;
            ControlVarEnd = controlVarEnd;
        }

        public override string GetLiteralRepresentation()
        {
            return $"for {ControlVar.GetLiteralRepresentation()} = {ControlVarStart.GetLiteralRepresentation()}, {ControlVarEnd.GetLiteralRepresentation()}{LoopBlock.GetLiteralRepresentation()}\nend";
        }
        public override LocalVar? FindLocal(string name)
        {
            return LoopBlock.FindLocal(name)??(ControlVar.Name == name ? ControlVar : null);
        }
    }

    class ForEachStatement : LoopStatement
    {
        public LocalVar[] Vars { get; }
        public ExpressionList IteratedExp { get; }
        public ForEachStatement(IEnumerable<LocalVar> vars, ExpressionList iteratedExp)
        {
            Vars = vars.ToArray();
            Vars.ForEach(t => t.Parent = this);
            IteratedExp = iteratedExp;
            IteratedExp.Parent = this;
        }

        public override LocalVar? FindLocal(string name)
        {
            return LoopBlock.FindLocal(name)??Vars.LastOrDefault(t => t.Name == name);
        }
        public override string GetLiteralRepresentation()
        {
            return $"for {Vars.ConcatStringList()} in {IteratedExp.GetLiteralRepresentation()} do\n{LoopBlock.GetLiteralRepresentation()}\nend";
        }
    }

    class VarDeclare : Statement
    {
        public LocalVar[] Vars { get; }
        public Expression[]? Expressions { get; }
        public VarDeclare(IEnumerable<string> names, IEnumerable<Expression> expressions)
        {
            Vars = names.Select(t => new LocalVar(t, null)).ToArray();
            Expressions = expressions.ToArray();
            Expressions.ForEach(t => t.Parent = this);
        }
        public VarDeclare(IEnumerable<string> names)
        {
            Vars = names.Select(t => new LocalVar(t, null)).ToArray();
        }

        public override string GetLiteralRepresentation()
        {
            return $"local {Vars.ConcatStringList()}{(Expressions != null ? " = " + Expressions.ConcatStringList() : "")}";
        }
    }

    sealed class DoStatement : Block
    {
        public override string GetLiteralRepresentation()
        {
            return $"do\n{base.GetLiteralRepresentation()}\nend";
        }
    }

    public enum FunctionDeclareType
    {
        Instance,
        Static
    }

    static partial class OperatorExtensions
    {
        public static string GetLiteralRepresentation(this FunctionDeclareType type)
        {
            return type switch {
                FunctionDeclareType.Instance => ".",
                FunctionDeclareType.Static => ":",
                _ => throw new()
            };
        }
    }
    
    sealed class CompoundNameFunctionDeclare : Block, IFunctionDefinition
    {
        public FunctionDeclareType FunctionDeclareType { get; }
        public Expression? FunctionNameExpression { get; }
        public string FunctionName { get; }
        public CompoundNameFunctionDeclare(FunctionDeclareType functionDeclareType, Expression? functionNameExpression, string functionName)
        {
            FunctionDeclareType = functionDeclareType;
            FunctionNameExpression = functionNameExpression;
            FunctionName = functionName;
            ParameterList = new(this);
        }
        public ParameterList ParameterList { get; }
        public override string GetLiteralRepresentation()
        {
            string functionName = FunctionNameExpression is null ? $"{FunctionNameExpression}{FunctionDeclareType.GetLiteralRepresentation()}" : FunctionDeclareType.GetLiteralRepresentation();
            string functionTitle = $"function {functionName}{FunctionName}{ParameterList.GetLiteralRepresentation()}\n{base.GetLiteralRepresentation()}\nend";
            return functionTitle;
        }
    }

    sealed class LocalFunctionDeclare : Block, IFunctionDefinition
    {
        public string FunctionName { get; }
        public LocalFunctionDeclare(string functionName)
        {
            FunctionName = functionName;
            ParameterList = new(this);
        }
        public ParameterList ParameterList { get; }
        public override string GetLiteralRepresentation() => $"local function {FunctionName}{ParameterList.GetLiteralRepresentation()}\n{base.GetLiteralRepresentation()}\nend";
    }

    sealed class RepeatStatement : LoopStatement
    {
        public Expression Predicate { get; }
        public RepeatStatement(Expression predicate)
        {
            Predicate = predicate;
            predicate.Parent = this;
        }

        public override LocalVar? FindLocal(string name)
        {
            return LoopBlock.FindLocal(name);
        }
        public override string GetLiteralRepresentation()
        {
            return $"repeat {LoopBlock.GetLiteralRepresentation()}\nuntil\n{Predicate.GetLiteralRepresentation()}\nend";
        }
    }
}
