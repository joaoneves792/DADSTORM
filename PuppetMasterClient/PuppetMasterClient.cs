using PuppetMasterLibrary;
using System;

namespace PuppetMasterClient
{
    class PuppetMasterClient {
		static void Main(string[] args) {

			// LEGACY puppetmaster now requires events to print
			// just copy from form

			PuppetMaster pm = new PuppetMaster();
			string fileNames = string.Join("", args);

			// Close all processes when ctrl+c is pressed
			Console.CancelKeyPress += new ConsoleCancelEventHandler(pm.CloseProcesses);
			
			while (true) {
				pm.Run(fileNames);
				fileNames = Console.ReadLine();
			}
		}
	}

}
