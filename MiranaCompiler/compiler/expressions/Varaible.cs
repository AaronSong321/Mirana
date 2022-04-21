namespace MiranaCompiler.Tree
{
    abstract class Variable : SyntaxTree
    {
        public Variable(string name, MiranaType? declareType)
        {
            Name = name;
            DeclareType = declareType;
        }

        public string Name { get; }
        public MiranaType? DeclareType { get; }
        public MiranaType? UsageType { get; set; }
        public bool IsCompilerGenerated { get; set; }

        public MiranaType Type {
            get => DeclareType??UsageType??ConcreteType.Any;
        }
        public override string GetLiteralRepresentation()
        {
            var type = DeclareType??UsageType;
            return type is null ? Name : $"{Name}: {type.GetLiteralRepresentation()}";
        }
    }

    class LocalVar: Variable
    {
        public LocalVar(string name, MiranaType? declareType) : base(name, declareType)
        {
        }
    }

    class GlobalVar: Variable
    {
        public GlobalVar(string name, MiranaType? declareType) : base(name, declareType)
        {
        }
    }

    class ParameterDefinition : LocalVar
    {
        public IFunctionDefinition DeclaringLiteralFunction { get; }
        public ParameterDefinition(string name, MiranaType? declareType, IFunctionDefinition declaringLiteralFunction) 
            : base(name, declareType)
        {
            DeclaringLiteralFunction = declaringLiteralFunction;
        }
    }

}