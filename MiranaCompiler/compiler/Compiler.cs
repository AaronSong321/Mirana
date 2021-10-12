using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;


namespace MiranaCompiler
{

    public class Compiler
    {
        public static readonly Version Version = new(1, 6, 1);
        public Compiler() { }
        static void State(CompileUnit t)
        {
            if (t.HasError()) {
                Console.WriteLine($"{t.ErrorNumber} errors in file {t.FilePath}");
                t.PrintErrors();
            }
        }
        public async Task CompileAsync(string[] paths)
        {
            var files1 = paths.Where(File.Exists);
            var files2 = paths.Where(Directory.Exists).SelectMany(M.GetFilePaths);
            var files = files1.Concat(files2).Distinct();
            var compileUnits = files.Select(file => new CompileUnit(file)).ToArray();
            var tasks = compileUnits.Select(t => Task.Run(() => {
                t.Compile();
                State(t);
            }));
            await Task.WhenAll(tasks);
        }
        public void Compile(string[] paths)
        {
            var files1 = paths.Where(File.Exists);
            var files2 = paths.Where(Directory.Exists).SelectMany(M.GetFilePaths);
            var files = files1.Concat(files2).Distinct();
            var compileUnits = files.Select(file => new CompileUnit(file)).ToArray();
            Parallel.ForEach(compileUnits, unit => {
                unit.Compile();
                State(unit);
            });
        }
    }
}
