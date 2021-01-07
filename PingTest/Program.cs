using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int packetsCount = 4; // количество ICMP-сообщений, которое будет отправлено
            int timeout = 10000; // максимально время ожидания ответа на ICMP-запрос
            int ttl = 128; // время жизни пакета
            Ping pingSender = new Ping();
            PingOptions op = new PingOptions();
            op.Ttl = ttl;
            op.DontFragment = true;

            int packetsSent = 0, packetsReceived = 0; //packetsSent - кол-во отправленных пакетов, packetsReceived - кол-во полученных пакетов
            long minRoundtripTime = long.MaxValue, maxRoundtripTime = 0;

            Console.WriteLine("Enter IP-address or domain name");
            string ipOrDns = Console.ReadLine();
            string data = "testpingtestpingtestpingtestping"; // размер данных - 32 байта
            byte[] buff = Encoding.ASCII.GetBytes(data);

            IPAddress ip;
            IPAddress.TryParse(ipOrDns, out ip);
            PingReply reply;

            bool writeTimes = true;

            Console.Write("Pinging ");
            if (ip == null)
            {
                ip = Dns.GetHostEntry(ipOrDns).AddressList[0];
                Console.Write(ipOrDns + " [" + ip.ToString() + "]");
            }
            else Console.Write(ip.ToString());
            Console.WriteLine(" with " + buff.Length + " bytes of data:");

            for (int i = 0; i < packetsCount; i++)
            {
                reply = pingSender.Send(ip, timeout, buff, op);
                packetsSent++;
                if (reply.Status == IPStatus.Success)
                {
                    packetsReceived++;
                    if (reply.RoundtripTime < minRoundtripTime)
                        minRoundtripTime = reply.RoundtripTime;
                    if (reply.RoundtripTime > maxRoundtripTime)
                        maxRoundtripTime = reply.RoundtripTime;
                    Console.WriteLine("Reply from " + reply.Address.ToString() + " bytes=" + buff.Length +
                        " time=" + reply.RoundtripTime + " TTL=" + op.Ttl);
                    continue;
                }
                writeTimes = false;
                if (reply.Status == IPStatus.DestinationHostUnreachable)
                {
                    packetsReceived++;
                    Console.WriteLine("Reply from " + reply.Address.ToString() + ": Destination host unreachable.");
                }
                if(reply.Status == IPStatus.TimedOut)
                    Console.WriteLine("Request timed out.");
            }
            Console.WriteLine("\nPing statistics for " + ip.ToString() + ":");
            Console.WriteLine("    Packets: Sent = " + packetsSent + ", Received = " + packetsReceived +
                ", Lost = " + (packetsSent - packetsReceived) +
                " (" + (double)(packetsSent - packetsReceived) / packetsCount * 100.0 + "% loss),");

            if (writeTimes)
            {
                Console.WriteLine("Approximate round trip times in milli-seconds:");
                Console.WriteLine("    Minimum = " + minRoundtripTime + "ms, " +
                    "Maximum = " + maxRoundtripTime + "ms, Average = " + (maxRoundtripTime + minRoundtripTime) / 2 + "ms");
            }
            Console.ReadLine();
        }
    }
}
