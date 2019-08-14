using System;
using CommandLine;
using CFG = Transmission.PushbulletImport.Configuration;

namespace Transmission.PushbulletImport.Commands
{
    public class Pull
    {
        [Verb("pull", HelpText = "Pulls in new messages")]
        public class PullOptions
        {
            
        }

        public static int Execute(PullOptions options)
        {
            var pbClient = CFG.Configuration.Default.PushbulletClient;
            
            
            
            throw new NotImplementedException();
        }
    }
}