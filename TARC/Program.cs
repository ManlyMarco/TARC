using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TARC;
using TARC.Compiler;

namespace TARC
{
    class Program
    {
        static void Main(string[] args)
        {
            MarkupCompiler compiler = new MarkupCompiler();


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var arc = compiler.CompileArchive(@"D:\TL conversion");
            stopwatch.Stop();

            Console.OutputEncoding = Encoding.UTF8;
            

            //foreach (var line in arc.Sections.SelectMany(x => x.Lines))
            //    Console.WriteLine($"{line.OriginalLine} = {line.TranslatedLine}");

            Console.WriteLine($"Compiled {arc.Sections.Select(x => x.Lines.Count).Sum()} lines in {stopwatch.Elapsed.TotalSeconds} seconds");
            Console.ReadLine();
        }
    }
}
