using System;
using System.Linq;
using CommandLine;
using PushbulletSharp.Filters;
using PushbulletSharp.Models.Responses;
using CFG = Transmission.PushbulletImport.Configuration;

namespace Transmission.PushbulletImport.Commands
{
    public class Pull
    {
        [Verb("pull", HelpText = "Pulls in new messages")]
        public class PullOptions
        {
            [Option('s',"since", Required = false)]
            public DateTime Since { get; set; } = DateTime.Now.AddMinutes(-30);
        }

        public static int Execute(PullOptions options)
        {
            var pbClient = CFG.Configuration.Default.PBClient;

            var pushFilter = new PushResponseFilter()
            {
                ModifiedDate = options.Since,
                IncludeTypes = new[] { PushResponseType.File, PushResponseType.Link}
            };
            
            var pushes = pbClient.GetPushes(pushFilter).Pushes;

            pushes.ForEach(ProcessPush);

            return Program.ExitNormal;
        }

        private static void ProcessPush(PushResponse push)
        {
            switch (push.Type)
            {
                case PushResponseType.File:
                    ProcessTorrentFile(push);
                    return;
                case PushResponseType.Link:
                    ProcessMagnetLink(push);
                    return;
                default:
                    // we shouldn't even be here, because of the push filter, so just ignore this
                    return;
            }
        }

        private static void ProcessMagnetLink(PushResponse push)
        {
            throw new NotImplementedException();
        }

        private static void ProcessTorrentFile(PushResponse push)
        {
            throw new NotImplementedException();
        }
    }
}