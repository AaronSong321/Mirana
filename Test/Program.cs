using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MiranaCompiler;


var compiler = new Compiler();
await compiler.CompileAsync(new [] {
    @"F:\Steam\steamapps\common\dota 2 beta\game\dota\scripts\vscripts\bots",
});

