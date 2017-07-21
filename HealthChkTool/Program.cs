using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Threading;

namespace HealthChkTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string targetIp = "8.8.8.8";
            string icmpRslts; //maybe for logging use if this will capture values in an array
            int val = 0; //test value for loop

            Ping PingSender = new Ping();

            //Ping options -  tweak as needed
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "12345678901234567890123456789012";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;

            while (val == 0) {
                PingReply reply = PingSender.Send(targetIp, timeout, buffer, options);

                if (reply.Status == IPStatus.Success)
                {
                    //Outputs to console
                    Console.WriteLine("Address: {0}", reply.Address.ToString());
                    Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                    Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                    Console.WriteLine("Don't fragement: {0}", reply.Options.DontFragment);
                    Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                }
                Thread.Sleep(10000); //sleep for 10 seconds so we dont spam the console and make pings without flooding.
                //NOTE - Could also use this to hold a value for user to specify the amount of time between 'tests'.
            }

        }
    }

    //Todo. 
    //This should check an array of results and if there's an error for say... 5 times we jump out the loop and execute this lot.
    public class Smtp
    {

    }
}
