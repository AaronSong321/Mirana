using System.Collections.Generic;

namespace MiranaCompiler.Tree
{
    interface IVarBlock
    {
        void AddLocal(LocalVar variable);
        LocalVar? FindLocal(string name);
        void AddStatement(Statement statement);
        ReturnStatement? ReturnStatement { get; set; }
        IEnumerable<Statement> GetStatements();
    }

    interface IFunctionDefinition : IVarBlock
    {
        ParameterList ParameterList { get; }
        public void AddParameter(string name, MiranaType? declaringType)
        {
            ParameterList.AddParameter(name, declaringType);
        }
        public ParameterDefinition? FindParameter(string name)
        {
            return ParameterList.FindParameter(name);
        }
    }
}
