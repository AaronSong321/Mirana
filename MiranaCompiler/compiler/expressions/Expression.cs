using System.Collections.Generic;
using System.Linq;

namespace MiranaCompiler.Tree
{
    abstract class WrittenTree : SyntaxTree
    {
        protected WrittenTree(SourceLocation? sourceLocation) : base(sourceLocation)
        {
        }
    }

    abstract class Expression : SyntaxTree
    {
        public Expression() : base(null)
        {
        }
        public Expression(SourceLocation? sourceLocation) : base(sourceLocation)
        {
        }
        public abstract MiranaType InferType();
    }

    abstract class AtomExpression : Expression
    {
        public AtomExpression() : base()
        {
            
        }
        public AtomExpression(SourceLocation? sourceLocation) : base(sourceLocation)
        {
        }
    }

    abstract class LiteralExpression : AtomExpression
    {
        public LiteralExpression()
        {
        }
    }

    class NullExpression : LiteralExpression
    {
        public string SourceString { get; }
        public NullExpression(string sourceString) : base()
        {
            SourceString = sourceString;
        }
        public override string GetLiteralRepresentation() => SourceString;
        public override MiranaType InferType() => ConcreteType.Null;
    }

    class BoolExpression : LiteralExpression
    {
        public string SourceString { get; }
        public BoolExpression(string sourceString) : base()
        {
            SourceString = sourceString;
        }
        public override string GetLiteralRepresentation() => SourceString;
        public override MiranaType InferType() => ConcreteType.Bool;
    }

    class StringLiteral : LiteralExpression
    {
        public string SourceSourceString { get; }
        public StringLiteral(string sourceString) : base()
        {
            SourceSourceString = sourceString;
        }
        public override string GetLiteralRepresentation() => $"\"{SourceSourceString.Replace("\"", "\\\"")}\"";
        public override MiranaType InferType() => ConcreteType.String;
    }

    class VarReference : AtomExpression
    {
        public VarReference(Variable referredVar, SourceLocation? sourceLocation) : base(sourceLocation)
        {
            ReferredVar = referredVar;
        }

        public override string GetLiteralRepresentation() => ReferredVar.GetLiteralRepresentation();
        public Variable ReferredVar { get; }
        public override MiranaType InferType()
        {
            return ReferredVar.Type;
        }
    }

    class LiteralFunction : LiteralExpression, IFunctionDefinition
    {
        public Block Block { get; }
        public ParameterList ParameterList { get; }
        public ReturnStatement? ReturnStatement {
            get => Block.ReturnStatement;
            set => Block.ReturnStatement = value;
        }

        public LiteralFunction()
        {
            Block = new() {
                Parent = this
            };
            ParameterList = new(this);
        }
        public override string GetLiteralRepresentation()
        {
            return $"function{ParameterList.GetLiteralRepresentation()}\n{Block.GetLiteralRepresentation()}\nend";
        }
        public override MiranaType InferType()
        {
            throw new System.NotImplementedException();
        }

        public void AddStatement(Statement statement)
        {
            Block.NonFinalStatements.Add(statement);
            statement.Parent = Block;
        }
        public IEnumerable<Statement> GetStatements()
        {
            foreach (var statement in Block.NonFinalStatements) {
                yield return statement;
            }
            if (ReturnStatement is not null)
                yield return ReturnStatement;
        }
        
        private readonly List<LocalVar> locals = new();
        public void AddLocal(LocalVar variable)
        {
            locals.Add(variable);
            variable.Parent = this;
        }
        public LocalVar? FindLocal(string name)
        {
            return locals.LastOrDefault(t => t.Name == name)??ParameterList.FindParameter(name);
        }
    }

    class DotsExpression : LiteralExpression
    {
        public override string GetLiteralRepresentation() => "...";
        public override MiranaType InferType() => ConcreteType.Any;
    }

    abstract class IndexExpression : Expression
    {
        public Expression Left { get; }
        public IndexExpression(Expression left)
        {
            Left = left;
        }
    }

    // class DotIndexExpression : IndexExpression
    // {
    //     public StringLiteral Right { get; }
    //     public DotIndexExpression(Expression left, StringLiteral right) : base(left)
    //     {
    //         Right = right;
    //     }
    //     public override string GetLiteralRepresentation() => $"{Left.GetLiteralRepresentation()}.{Right.GetLiteralRepresentation()}";
    //     public override MiranaType InferType()
    //     {
    //         var leftType = Left.InferType();
    //         MiranaType targetType;
    //         if (leftType is ConcreteLuaObjectType t1) {
    //             
    //         }
    //     }
    // }

    // class BracketIndexExpression : IndexExpression
    // {
    //     public override string GetLiteralRepresentation() => $"{Left.GetLiteralRepresentation()}[{Right.GetLiteralRepresentation()}]";
    // }
}

// | prefixexp
//     | ifexp
//     | number
//     | string
//     | dots
//     | it
//     | lambdaImplicitParam
//     | tableconstructor
//     | kotLambda
//     | operatorLambda
//     | <assoc=right> exp operatorPower exp
//     | operatorUnary exp
//     | exp operatorMulDivMod exp
//     | exp operatorAddSub exp
//     | <assoc=right> exp operatorStrcat exp
//     | exp operatorComparison exp
//     | exp operatorAnd exp
//     | exp operatorOr exp
//     | exp OpAmpersand exp
//     | exp OpPipe exp
//     | exp OpTilde exp
//     | exp (OpShl | OpShr) exp
// expBlock: stat* (retstat | explist)?;
//
// prefixexp: varOrExp nameAndArgs*;
//
// functioncall: varOrExp nameAndArgs+;
//
// varOrExp: assignableExp | '(' exp ')';
//
// assignableExp: (NAME | it | lambdaImplicitParam | '(' exp ')' varSuffix) varSuffix*;
//
// varSuffix: nameAndArgs* ('[' exp ']' | OpDot NAME);
// nameAndArgs: (OpColon NAME)? args;
//
// args
//     : '(' explist? ')' kotLambda
//     | kotLambda
//     | '(' explist? ')'
//                    | tableconstructor 
//                    | string
//     ;
//
// funcLiteral
//     : FUNCTION funcbody
//     ;
//
// funcbody: '(' parlist? ')' block END;
//
// parlist: namelist (OpComma dots)? | dots;
//
// tableconstructor: LBRACE fieldlist? RBRACE;
//
// fieldlist: field (fieldsep field)* fieldsep?;