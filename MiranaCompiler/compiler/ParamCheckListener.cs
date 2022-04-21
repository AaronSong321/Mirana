using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiranaCompiler.grammar;

namespace MiranaCompiler.grammar
{
    partial class miranaParser
    {
        partial class LambdaImplicitParamContext
        {
            public int Num => int.Parse(GetText()[1..2]);
        }

        partial class FunLambdaContext
        {
            internal readonly SortedSet<int> usedPar = new();
            private bool usedIt;
            internal void UseIt() => usedIt = true;

            internal bool ParItConflict => usedPar.Count != 0 && usedIt;
            internal string CreateParamListString()
            {
                if (usedPar.Count == 0) {
                    return usedIt ? $"({LuaWriter.GetLambdaImplicitIterName()})" : "()";
                }
                int num = usedPar.Max();
                StringBuilder sb = new("(");
                var pars = Enumerable.Range(1, num).Select(index => usedPar.Contains(index) ? LuaWriter.GetLambdaImplicitParamName(index) : "_").ToArray();
                if (pars.Length > 0) {
                    sb.Append(pars[0]);
                    foreach (var par in pars.Skip(1)) {
                        sb.Append(", ");
                        sb.Append(par);
                    }
                }
                sb.Append(")");
                return sb.ToString();
            }
        }
    }
}

namespace MiranaCompiler
{
    // internal class ParamCheckListener: miranaBaseListener
    // {
    //     private readonly miranaParser.FunLambdaContext funLambdaContext;
    //     private readonly CompileUnit compileUnit;
    //     public ParamCheckListener(miranaParser.FunLambdaContext context, CompileUnit compileUnit)
    //     {
    //         funLambdaContext = context;
    //         this.compileUnit = compileUnit;
    //     }
    //
    //     private int recordPause = 0;
    //     public override void EnterFunLambda(miranaParser.FunLambdaContext context)
    //     {
    //         if (context != funLambdaContext)
    //             ++recordPause;
    //     }
    //
    //     public override void ExitFunLambda(miranaParser.FunLambdaContext context)
    //     {
    //         if (context != funLambdaContext)
    //             --recordPause;
    //     }
    //
    //     public override void EnterLambdaImplicitParam(miranaParser.LambdaImplicitParamContext context)
    //     {
    //         if (recordPause == 0)
    //             funLambdaContext.usedPar.Add(context.Num);
    //     }
    //
    //     public override void EnterIt(miranaParser.ItContext context)
    //     {
    //         if (recordPause == 0) {
    //             funLambdaContext.UseIt();
    //         }
    //     }
    // }
}
