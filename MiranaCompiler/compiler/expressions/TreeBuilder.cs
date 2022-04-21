// using System.Collections.Generic;
// using System.Linq;
// using Antlr4.Runtime.Tree;
// using MiranaCompiler.grammar;
//
// namespace MiranaCompiler.Tree
// {
//     public class TreeBuilder
//     {
//         private readonly Stack<IVarBlock> blocks = new();
//         private int tempVarNum = 0;
//         private static string GetTempVarName(int tempVarIndex) => $"_V_{tempVarIndex}";
//         private LocalVar NewTempVar()
//         {
//             var topBlock = blocks.Peek();
//             string name = GetTempVarName(++tempVarNum);
//             LocalVar local = new(name, null) {
//                 IsCompilerGenerated = true
//             };
//             topBlock.AddLocal(local);
//             return local;
//         }
//         private void PushStatement(Statement statement)
//         {
//             blocks.Peek().AddStatement(statement);
//         }
//         private LocalVar StoreInTempVar(Expression expression)
//         {
//             AssignState statement = new();
//         }
//         
//         public void Build(miranaParser.ChunkContext context)
//         {
//             Chunk chunk = new();
//             blocks.Clear();
//             blocks.Push(chunk);
//             
//         }
//
//         // Statement Build(miranaParser.StatContext context)
//         // {
//         //     
//         // }
//
//         Statement Build(miranaParser.Stat_assignContext context)
//         {
//             AssignState statement = new();
//             context.varlist().assignableExp().Select(Build);
//         }
//
//         Expression Build(miranaParser.ExpContext context)
//         {
//             if (context.NIL() != null) {
//                 return BuildLiteralNull(context.NIL());
//             }
//             if (context.TRUE() != null || context.FALSE() != null) {
//                 return BuildLiteralBool(context.TRUE()??context.FALSE());
//             }
//             
//         }
//
//         NullExpression BuildLiteralNull(ITerminalNode node)
//         {
//             return new(node.GetText()) {
//                 SourceLocation = new(node)
//             };
//         }
//
//         BoolExpression BuildLiteralBool(ITerminalNode node)
//         {
//             return new(node.GetText()) {
//                 SourceLocation = new(node)
//             };
//         }
//
//         Expression Build(miranaParser.PrefixexpContext context)
//         {
//             var v = context.varOrExp();
//             var v2 = context.nameAndArgs();
//         }
//
//         Expression Build(miranaParser.VarOrExpContext context)
//         {
//             if (context.assignableExp() != null) {
//                 return Build(context.assignableExp());
//             }
//             return Build(context.exp());
//         }
//         
//         Expression Build
//     }
// }
