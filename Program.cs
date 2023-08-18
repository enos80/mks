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
        static bool qbittorrentRunning;
        static bool vpnRunning;
        static bool killSwitch = true;
        static string? msg;
        static readonly string? exitNode = "us-chi-wg-104";

        static async Task Main()
        {
            while (killSwitch)
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
                    KillqBittorrent();
                    killSwitch = false;
                }

                vpnRunning = IsVpnActive(mullvad);
                qbittorrentRunning = IsQBTRunning();

                if (vpnRunning == true && qbittorrentRunning == true && mullvad.MullvadHostname == exitNode)
                {
                    msg = $"Mullvad and qBittorrent active. Exit Node: {mullvad.MullvadHostname}";
                }
                else
                {
                    msg = $"Mullvad: {vpnRunning}. qBittorrent: {qbittorrentRunning}. Exit Node: {mullvad.MullvadHostname}";
                    MksShutdown(msg);
                }
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

        static void MksShutdown(string msg)
        {
            KillqBittorrent();
            killSwitch = false;
            Console.WriteLine(msg);
            Environment.Exit(0);
        }
    }
}