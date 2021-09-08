using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MiranaCompiler;

const string src = "../../../../MiranaCompiler/bin/Release/net5.0";
string dst = Path.GetFullPath($"{src}/Mirana-{Compiler.Version}.zip");
if (Directory.Exists(src)) {
    File.Delete(dst);
    ZipFile.CreateFromDirectory(src, dst);
}

var compiler = new Compiler();
await compiler.CompileAsync(new [] {
    @"F:\Steam\steamapps\common\dota 2 beta\game\dota\scripts\vscripts\bots",
});

