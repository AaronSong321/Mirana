using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace MiranaCompiler
{
    public static class M
    {
        public static void ForEach<T>(this IEnumerable<T> a, Action<T> func)
        {
            foreach (var b in a)
                func(b);
        }

        public static IEnumerable<string> GetFilePaths(this string path)
        {
            var a = new DirectoryInfo(path);
            List<string> g = new();
            
            void Find(DirectoryInfo d)
            {
                d.GetFiles().Where(t => Regex.IsMatch(t.FullName, ".*\\.mira")).Select(t => t.FullName).ForEach(g.Add);
                d.GetDirectories().ForEach(Find);
            }

            Find(a);
            return g;
        }

        public static bool IsIdentifierLetter(this char c)
        {
            return char.IsLetterOrDigit(c) || c is '_';
        }
    }

    public class Compiler
    {
        public static readonly Version Version = new(1, 5, 1);
        public Compiler() { }
        public async Task CompileAsync(string[] paths)
        {
            var files1 = paths.Where(File.Exists);
            var files2 = paths.Where(Directory.Exists).SelectMany(M.GetFilePaths);
            var files = files1.Concat(files2).Distinct().ToArray();
            var compileUnits = files.Select(file => new CompileUnit(file)).ToArray();
            static void State(CompileUnit t)
            {
                if (t.HasError()) {
                    Console.WriteLine($"{t.ErrorNumber} errors in file {t.FilePath}");
                    t.PrintErrors();
                }
                else {
                    Console.WriteLine($"Successfully write to file {MiranaToLuaWriter.CreateLuaFilePath(t.FilePath)}");
                }
            }
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
            var files = files1.Concat(files2).ToArray();
            var compileUnits = files.Select(file => new CompileUnit(file)).ToArray();
            compileUnits.ForEach(t => t.Compile());
        }
    }
}
