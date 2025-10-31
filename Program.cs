using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace rm
{
    internal static class Program
    {
        private const string HelpText = @"Usage: rm [OPTION]... FILE...
Delete files or directories.

Options:
  -r, --recursive   remove directories and their contents recursively
  -f, --force       ignore nonexistent files and arguments, never prompt
  -h, --help        display this help and exit
  --                stop parsing flags (treat remaining args as file names)
Examples:
  rm file.txt             Delete file.txt
  rm -rf build            Recursively and forcibly delete 'build' directory
  rm -f *.log             Force delete all .log files in current directory";

        private static int Main(string[] args)
        {
            if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
            {
                Console.WriteLine(HelpText);
                return 0;
            }

            bool recurse = false;
            bool force = false;
            bool stopFlags = false;
            var rawTargets = new List<string>();

            foreach (var a in args)
            {
                if (!stopFlags && a == "--") { stopFlags = true; continue; }

                if (!stopFlags && a.StartsWith("-", StringComparison.Ordinal))
                {
                    foreach (var ch in a.Skip(1))
                    {
                        if (ch == 'r') recurse = true;
                        else if (ch == 'f') force = true;
                    }
                    continue;
                }

                rawTargets.Add(a);
            }

            if (rawTargets.Count == 0)
            {
                Console.Error.WriteLine("rm: missing operand");
                Console.Error.WriteLine("Try 'rm --help' for more information.");
                return 1;
            }

            var targets = ExpandTargets(rawTargets);
            int exitCode = 0;

            foreach (var t in targets)
            {
                try
                {
                    DeletePath(t, recurse, force);
                }
                catch (Exception ex)
                {
                    if (!force)
                    {
                        Console.Error.WriteLine($"rm: cannot remove '{t}': {ex.Message}");
                        exitCode = 1;
                    }
                }
            }

            return exitCode;
        }

        private static IEnumerable<string> ExpandTargets(IEnumerable<string> inputs)
        {
            var results = new List<string>();
            foreach (var input in inputs)
            {
                var path = input;
                if (path.StartsWith("~"))
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    path = Path.Combine(home, path.Substring(1).TrimStart('\\', '/'));
                }

                if (path.Contains('*') || path.Contains('?'))
                {
                    var dir = Path.GetDirectoryName(path);
                    var pat = Path.GetFileName(path);
                    if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();
                    try
                    {
                        var matches = Directory.GetFileSystemEntries(dir, pat, SearchOption.TopDirectoryOnly);
                        results.AddRange(matches.Length > 0 ? matches : new[] { path });
                    }
                    catch
                    {
                        results.Add(path);
                    }
                }
                else
                {
                    results.Add(path);
                }
            }
            return results;
        }

        private static void DeletePath(string path, bool recurse, bool force)
        {
            var full = path;
            try { full = Path.GetFullPath(path); } catch { }

            if (!File.Exists(full) && !Directory.Exists(full))
            {
                if (!force) throw new FileNotFoundException("No such file or directory");
                return;
            }

            var attr = File.GetAttributes(full);
            bool isReparse = (attr & FileAttributes.ReparsePoint) != 0;
            bool isDir = (attr & FileAttributes.Directory) != 0;

            if (!isDir)
            {
                if (force) ClearReadOnly(full, false);
                File.Delete(full);
                return;
            }

            if (isReparse)
            {
                Directory.Delete(full);
                return;
            }

            if (!recurse) throw new IOException("is a directory");

            if (force) ClearReadOnly(full, true);
            Directory.Delete(full, true);
        }

        private static void ClearReadOnly(string path, bool isDir)
        {
            if (!isDir)
            {
                try
                {
                    var attrs = File.GetAttributes(path);
                    if ((attrs & FileAttributes.ReadOnly) != 0)
                        File.SetAttributes(path, attrs & ~FileAttributes.ReadOnly);
                }
                catch { }
                return;
            }

            try
            {
                foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var a = File.GetAttributes(f);
                        if ((a & FileAttributes.ReadOnly) != 0)
                            File.SetAttributes(f, a & ~FileAttributes.ReadOnly);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
