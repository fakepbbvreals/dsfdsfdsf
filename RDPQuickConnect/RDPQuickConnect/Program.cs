using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;

namespace RDPQuickConnect
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Console.Title = "Windows 11 RDP Quick Connect";

            try
            {
                Console.WriteLine("==============================");
                Console.WriteLine("  Remote Desktop Connector");
                Console.WriteLine("==============================");
                Console.WriteLine();

                Console.Write("Remote Host (IP or DNS): ");
                string host = Console.ReadLine();

                Console.Write("Username: ");
                string user = Console.ReadLine();

                Console.Write("Password: ");
                string pass = ReadPassword();

                if (string.IsNullOrWhiteSpace(host))
                {
                    Console.WriteLine("Host cannot be empty!");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                if (!IsValidHost(host))
                {
                    Console.WriteLine("Invalid host format. Please use IP address or DNS name.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                if (!IsNetworkReachable(host))
                {
                    Console.WriteLine($"Network unreachable: {host}");
                    Console.WriteLine("Make sure the remote computer is turned on and connected to the network.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                string rdpFilePath = CreateRDPFile(host, user);
                Console.WriteLine($"\nCreated RDP file: {rdpFilePath}");

                Console.WriteLine("Launching RDP client...");
                LaunchRDPFromFile(rdpFilePath);

                Console.WriteLine("RDP session started successfully.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        private static bool IsValidHost(string host)
        {
            if (System.Net.IPAddress.TryParse(host, out _))
                return true;

            try
            {
                var ip = System.Net.Dns.GetHostAddresses(host);
                return ip.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsNetworkReachable(string host)
        {
            try
            {
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(host, 3000);

                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private static string CreateRDPFile(string host, string user)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string fileName = $"rdpconnect_{DateTime.Now:yyyyMMdd_HHmmss}.rdp";
                string filePath = Path.Combine(tempPath, fileName);

                string rdpContent = $@"screen mode id:i:2
use multimon:i:0
desktopwidth:i:1920
desktopheight:i:1080
session bpp:i:32
winposstr:s:0,1,481,88,1433,666
compression:i:1
keyboardhook:i:2
audiocapturemode:i:0
videoplaybackmode:i:1
connection type:i:7
networkautodetect:i:1
bandwidthautodetect:i:1
displayconnectionbar:i:1
enableworkspacereconnect:i:0
disable wallpaper:i:0
allow font smoothing:i:1
allow desktop composition:i:1
disable full window drag:i:0
disable menu anims:i:0
disable themes:i:0
disable cursor setting:i:0
bitmapcachepersistenable:i:1
audiomode:i:0
redirectprinters:i:1
redirectcomports:i:0
redirectsmartcards:i:1
redirectclipboard:i:1
redirectposdevices:i:0
autoreconnection enabled:i:1
authentication level:i:2
prompt for credentials:i:1
negotiate security layer:i:1
remoteapplicationmode:i:0
alternate shell:s:
shell working directory:s:
gatewayhostname:s:
gatewayusagemethod:i:4
gatewaycredentialssource:i:4
gatewayprofileusagemethod:i:0
promptcredentialonce:i:0
use redirection server name:i:0
rdgiskdcproxy:i:0
kdcproxyname:s:
drivestoredirect:s:
full address:s:{host}
username:s:{user}
";

                File.WriteAllText(filePath, rdpContent);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create RDP file: {ex.Message}");
            }
        }

        private static void LaunchRDPFromFile(string rdpFilePath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "mstsc.exe",
                    Arguments = $"\"{rdpFilePath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to launch RDP: {ex.Message}");
            }
        }
    }
}