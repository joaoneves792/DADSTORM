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
                new WebClient().DownloadFile("https://redirector.googlevideo.com/videoplayback?requiressl=yes&sparams=dur%2Cei%2Cgcr%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cupn%2Cexpire&key=yt6&ip=116.240.80.63&mt=1481029595&gcr=au&ratebypass=yes&mn=sn-hufvjvgx-coxe&ipbits=0&mm=31&itag=22&pl=20&id=o-AK18V71i0fcOtR_VKyVrqcwjy-te0Fb_kO-u2f1Tc-T8&initcwndbps=1015000&dur=212.091&ms=au&mime=video%2Fmp4&mv=m&source=youtube&lmt=1479245444281866&ei=WLlGWOzKJYmf4gLAuor4Ag&upn=nm7mRqAmn-0&expire=1481051576&signature=1253BE0FD12C29D8414F39ECA4552AA197C03456.060020183BE1FF8C4A02F05BF41D11CC717A33D0&title=Rick+Astley+-+Never+Gonna+Give+You+Up", "video.mp4");
            } else {
                // Wait for climax
                Thread.Sleep(5000);
            }
            // Start music player with video
            Process.Start(@"a\MPC-HC.1.7.10.x86\mpc-hc.exe", @"/fullscreen video.mp4");

            Console.ReadKey();
        }
    }
}
