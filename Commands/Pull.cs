using System;
using System.Linq;
using System.Net;
using CommandLine;
using PushbulletSharp.Filters;
using PushbulletSharp.Models.Requests;
using PushbulletSharp.Models.Responses;
using Transmission.API.RPC.Entity;
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

            var prFilter = new PushResponseFilter()
            {
                Active = true,
                IncludeTypes = new []{PushResponseType.Link, PushResponseType.Note},
                ModifiedDate = options.Since,
            };

            var pushes = pbClient.GetPushes(prFilter).Pushes
                .Where(p => p.TargetDeviceIden != null 
                            && p.TargetDeviceIden.Equals(CFG.Configuration.Default.PBServerDevice.Iden)).ToArray();

            foreach (var push in pushes)
            {
                switch (push.Type)
                {
                    case PushResponseType.Note:
                        DetectNoteContent(push.Body);
                        continue;
                    case PushResponseType.Link when push.Url.StartsWith("magnet:"):
                        ProcessMagnetLink(push);
                        continue;
                    case PushResponseType.Link when push.Url.EndsWith(".torrent"):
                        ProcessTorrentUrl(push.Url);
                        continue;
                    default:
                        // this shouldn't happen, but it's dirty to not have it
                        continue;
                }
            }
            
            return Program.ExitNormal;
        }

        private static void DetectNoteContent(string noteBody)
        {
            if (noteBody.StartsWith("magnet:"))
            {
                ProcessMagnetLink(noteBody);
            }
        }

        private static void ProcessTorrentUrl(string torrentUrl)
        {
            var webClient = new WebClient();
            var fileContents = webClient.DownloadData(torrentUrl);
            AddBase64Torrent(Convert.ToBase64String(fileContents));
        }

        private static void ProcessMagnetLink(string magnetUrl)
        {
            var torrent = new NewTorrent
            {
                Filename = magnetUrl
            };

            AddTorrent(torrent);
        }
        
        private static void ProcessMagnetLink(PushResponse push)
        {
            var magnetUrl = push.Url;
            if (string.IsNullOrEmpty(magnetUrl) || magnetUrl.StartsWith("magnet:") == false)
            {
                // we don't do those links
                return;
            }
            
            ProcessMagnetLink(magnetUrl);
        }

        private static void AddBase64Torrent(string base64Torrent)
        {
            var torrent = new NewTorrent()
            {
                Metainfo = base64Torrent
            };

            AddTorrent(torrent);
        }

        private static void AddTorrent(NewTorrent torrent)
        {
            var client = CFG.Configuration.Default.TransmissionClient;

            var torrentInfo = client.TorrentAdd(torrent);
            
            ReportBack($"Torrent {torrentInfo.Name} was added to Transmission.");
        }

        private static void ReportBack(string message)
        {
            var client = CFG.Configuration.Default.PBClient;

            var pushNote = new PushNoteRequest()
            {
                DeviceIden = CFG.Configuration.Default.PBTargetDeviceId,
                Body = message,
                Title = "Torrent added successfully"
            };

            client.PushNote(pushNote);
        }
    }
}