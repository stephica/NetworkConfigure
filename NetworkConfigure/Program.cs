using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
namespace NetworkConfigure
{
    class Program
    {

        static void Main(string[] args)
        {
            

            NetworkInterface[] Interfaces = null;
            string[,] interfacesInfo = null;

            //IP adresses

            string gateway = "10.190.0.1";

            getInterfacesInfo(ref Interfaces, ref interfacesInfo);
            
            string[] macConfig = File.ReadAllLines(@"C:\ip2mac.txt", System.Text.Encoding.Default);


            string[,] macIpPairs = new string[macConfig.Length, 3];
            for (int i = 0; i < macConfig.Length; i++)
            {
                try
                {
                    string[] temp = macConfig[i].Split(',');

                    macIpPairs[i, 0] = temp[0];
                    macIpPairs[i, 1] = temp[1];
                    macIpPairs[i, 2] = temp[2];
                }
                catch (IndexOutOfRangeException)
                {
                    macIpPairs[i, 0] = "";
                    macIpPairs[i, 1] = "";
                    macIpPairs[i, 2] = "";
                    continue;
                }
            }


            for (int i = 0; i < interfacesInfo.GetLength(0); i++)
            {
                if ((interfacesInfo[i, 0] == "LAN" || interfacesInfo[i, 0] == "LAN1" || interfacesInfo[i, 0] == "LAN2")&& interfacesInfo[i,2] == "Up")
                {
                    int adapterConfingStringNumber = getIpIndexFromMac(macIpPairs, interfacesInfo[i, 1]);
                    
                    if (adapterConfingStringNumber != -1)
                    {
                        setIpforAdapter(interfacesInfo[i, 0], macIpPairs[adapterConfingStringNumber, 1], macIpPairs[adapterConfingStringNumber, 2], gateway);                           
                    }
                }

            }
            
    }


        static void setIpforAdapter(string interfaceName,string firstIP,string secondIP,string gateway)
        {
            Process cmd = new Process();

            cmd.StartInfo.FileName = @"C:\WINDOWS\system32\cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = false;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.Start();

            string mainDNS = "10.0.20.1";
            string secondaryDNS = "10.0.20.2";
            
            string netmask16 = "255.255.0.0";
            string netmask24 = "255.255.255.0";

            string metric = "0";
           
            //Setting IP
            cmd.StandardInput.WriteLine(@"netsh interface ip set address name=" + interfaceName + " static " + firstIP + " " + netmask24 + " " + gateway + " " + metric);
            cmd.StandardInput.WriteLine(@"netsh interface ip add address name=" + interfaceName + " " + secondIP + " " + netmask16);

            //Setting DNS

            cmd.StandardInput.WriteLine(@"netsh interface ip set dns name=" + interfaceName + " static " + mainDNS);
            cmd.StandardInput.WriteLine(@"netsh interface ip add dns name=" + interfaceName + " " + secondaryDNS);

        }

        static int getIpIndexFromMac(string[,] A, string MAC)
        {
            for (int i = 0; i < A.Length; i++)
            {
                if (A[i, 0] == MAC)
                {

                    return i;
                }

            }


            return -1;
        }

        static public void getInterfacesInfo(ref NetworkInterface[] Net, ref string[,] Info)
        {

            Net = NetworkInterface.GetAllNetworkInterfaces();
            Info = new string[Net.Length, 3];

            for (int i = 0; i < Net.Length; i++)
            {
                Info[i, 0] = Net[i].Name.ToString();
                Info[i, 1] = Net[i].GetPhysicalAddress().ToString();
                Info[i, 2] = Net[i].OperationalStatus.ToString();
            }
        }

    }
}
