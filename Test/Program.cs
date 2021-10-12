using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common;
using LibGit2Sharp;
using MiranaCompiler;

static IEnumerable<string> GetFilePaths(string path, string[] ignore)
{
    var a = new DirectoryInfo(path);
    List<string> g = new();
            
    void Find(DirectoryInfo d)
    {
        d.GetFiles().Where(t => !ignore.Contains(t.FullName)).Select(t => t.FullName).ForEach(g.Add);
        d.GetDirectories().Where(t => !ignore.Contains(t.FullName)).ForEach(Find);
    }

    Find(a);
    return g;
}


const string sirius = @"F:\Steam\steamapps\common\dota 2 beta\game\dota\scripts\vscripts\bots_sirius";
const string rmm = @"F:\Steam\steamapps\common\dota 2 beta\game\dota\scripts\vscripts\bots_rmm";
const string localDevelopmentDir = @"F:\Steam\steamapps\common\dota 2 beta\game\dota\scripts\vscripts\bots";

// GetFilePaths(localDevelopmentDir, Array.Empty<string>()).Where(t => t.EndsWith(".lua")).ForEach(t => CopyFileWithPath(t, t.Substring(0, t.Length - 4) + ".mira"));
// new Compiler().Compile(new [] { localDevelopmentDir });

static void CopyFileWithPath(string s, string d)
{
    if (Path.GetDirectoryName(d) is not null && !Directory.Exists(Path.GetDirectoryName(d)))
        Directory.CreateDirectory(Path.GetDirectoryName(d)!);
    File.Copy(s, d, true);
}
static void SwitchTo(string dir)
{
    // if (Directory.Exists(localDevelopmentDir))
    //     Directory.Delete(localDevelopmentDir, true);
    // Directory.CreateDirectory(localDevelopmentDir);
    // int dirLen = dir.Length;
    new Compiler().Compile(new[] { dir });
    // GetFilePaths(dir, new [] {
    //     Path.Combine(dir, ".git"),
    //     Path.Combine(dir, ".gitignore"),
    //     Path.Combine(dir, ".vscode")
    // }).Where(t => !t.EndsWith(".mira")).ForEach(t => CopyFileWithPath(t, localDevelopmentDir + t.Substring(dirLen, t.Length - dirLen)));
}
static void Compile(string dir)
{
    using Repository repository = new(dir);
    var status = repository.RetrieveStatus();
    new Compiler().Compile(status.Added.Concat(status.Modified).Concat(status.Untracked).Select(t => Path.Combine(dir, t.FilePath)).Where(t => t.EndsWith(".mira")).ToArray());
}
static void CopyFiles(string dir)
{
    using Repository repository = new(dir);
    var status = repository.RetrieveStatus();
    int dirLen = dir.Length;
    var modifiedFiles = status.Added.Concat(status.Modified).Concat(status.Untracked).Select(t => Path.Combine(dir, t.FilePath)).ToArray();
    modifiedFiles.Where(t => t.EndsWith(".lua")).ForEach(t => {
        string dst = localDevelopmentDir + t.Substring(dirLen, t.Length - dirLen);
        CopyFileWithPath(t, dst);
    });
    modifiedFiles.Where(t => t.EndsWith(".mira")).ForEach(t => {
        string src = t.Substring(0, t.Length - 5) + ".lua";
        string dst = localDevelopmentDir + src.Substring(dirLen, src.Length - dirLen);
        CopyFileWithPath(src, dst);
    });
}
// SwitchTo(localDevelopmentDir);
Compile(localDevelopmentDir);
// CopyFiles(sirius);

