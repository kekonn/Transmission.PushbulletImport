using System;
using CommandLine;
using Transmission.PushbulletImport.Commands;

namespace Transmission.PushbulletImport
{
    public static class Program
    {
        internal const int ExitNormal = 0;
        
        public static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<Pull.PullOptions>(args)
                .MapResult(
                    Pull.Execute,
                    errs => 1);
        }
    }
}
