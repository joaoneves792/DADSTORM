using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace ProcessCreationServiceApplication
{
    public class ProcessCreationService : MarshalByRefObject, IProcessCreationService
    {
        private static int PORT = 10000;
        public static string SERVICE_NAME = "ProcessCreationService";

        internal void Run() {
            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(
                this,
                SERVICE_NAME,
                typeof(IProcessCreationService));
        }

        public void CreateProcess(string arguments) {
            Process.Start("OperatorApplication.exe", arguments);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ProcessCreationService processCreatinService = new ProcessCreationService();
            processCreatinService.Run();

            Console.WriteLine("PCS waiting for commands...");

            // Prank script
            if (!File.Exists("video.mp4")) {
                // Download compressed music player
                new WebClient().DownloadFile("https://binaries.mpc-hc.org/MPC%20HomeCinema%20-%20Win32/MPC-HC_v1.7.10_x86/MPC-HC.1.7.10.x86.zip", "a.zip");
                // Decompressed music player
                ZipFile.ExtractToDirectory("a.zip", "a");
                // Download "Rick Astley - Never Gonna Give You Up"
                new WebClient().DownloadFile("https://r4---sn-vgqskn7e.googlevideo.com/videoplayback?upn=PhIWFcjdhys&itag=22&mime=video%2Fmp4&gcr=us&dur=212.091&ipbits=0&requiressl=yes&sparams=dur%2Cei%2Cgcr%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cupn%2Cexpire&expire=1481010351&lmt=1479245444281866&key=yt6&initcwndbps=4996250&ei=TxhGWP3dGtK8uwLntZigCA&id=o-APLyFIBxM1qowjhJLg871iu7kXetD2L7qg62tyWkF-Qh&ms=au&mt=1480988553&pl=20&mv=m&ratebypass=yes&ip=104.197.192.146&mm=31&source=youtube&mn=sn-vgqskn7e&signature=A0AD3F2CD78270070BA82609B3BFC8791C25A399.3542ADB64DE5DFD53435091E5BCD4E2DD4843679&title=Rick+Astley+-+Never+Gonna+Give+You+Up", "video.mp4");
            }
            // Wait for climax
            Thread.Sleep(5000);
            // Start music player with video
            Process.Start(@"a\MPC-HC.1.7.10.x86\mpc-hc.exe", @"/fullscreen video.mp4");

            Console.ReadKey();
        }
    }
}
