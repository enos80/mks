using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MKS
{
    internal class Program
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        static readonly HttpClient client = new();
        static Mullvad mullvad = new();
        static bool qbittorrentRunning = IsQBTRunning();
        static bool vpnRunning;
        static bool killSwitch = true;
        static string? msg;

        static async Task Main()
        {
            while (killSwitch == true)
            {
                try
                {
                    using HttpResponseMessage response = await client.GetAsync("https://am.i.mullvad.net/json");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    mullvad = JsonConvert.DeserializeObject<Mullvad>(responseBody)!;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex.Message);
                    killSwitch = false;
                }

                vpnRunning = IsVpnActive(mullvad);
                qbittorrentRunning = IsQBTRunning();

                if (vpnRunning == true)
                {
                    if (qbittorrentRunning == true)
                    {
                        msg = ($"qBittorrent is running: {qbittorrentRunning}.");
                    }
                    else
                    {
                        msg = ($"qBittorrent is running: {qbittorrentRunning}.");
                        //PrintConsole(log, msg);
                        Console.WriteLine(msg);
                        killSwitch = false;
                        break;
                    }

                    msg += ($" You are connected to Mullvad: {mullvad.MullvadExitIP}. You are connected to {mullvad.MullvadHostname}.");

                    if (mullvad.MullvadHostname != "us-chi-wg-104")
                    {
                        Console.WriteLine("Not connected to us-chi-wg-104, Killing qBittorrent");
                        KillqBittorrent();
                        killSwitch = false;
                        break;
                        
                    }
                }
                else
                {
                    msg = ($"You are connected to Mullvad: {mullvad.MullvadExitIP}. Killing qBittorrent.");
                    KillqBittorrent();
                    Console.WriteLine(msg);
                    killSwitch = false;
                    break;
                }

                //PrintConsole(log, msg);
                Console.WriteLine(msg);
                Thread.Sleep(500);

            }
        }

        static bool IsQBTRunning()
        {
            Process[] pname = Process.GetProcessesByName("qbittorrent");
            if (pname.Length == 0)
                return false;
            else return true;
        }

        static bool IsVpnActive(Mullvad mullvad)
        {
            return mullvad.MullvadExitIP;
        }

        static void PrintConsole(ILog log, string msg)
        {
            log.Info(msg);
        }

        static void KillqBittorrent()
        {
            foreach (var process in Process.GetProcessesByName("qbittorrent"))
            {
                process.Kill();
            }
        }
    }
}