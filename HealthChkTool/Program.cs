using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Threading;
using System.Net;

namespace HealthChkTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string targetIp = "1.2.4.5";
            string icmpRslts; //maybe for logging use if this will capture values in an array
            int val = 0; //test value for loop

            Ping PingSender = new Ping();

            //Ping options -  tweak as needed
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "12345678901234567890123456789012";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;

            while (val != 5) {
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
                else val = (++val);
                if (val == 5)
                {
                    SmtpClient smtpClient = new SmtpClient();
                    NetworkCredential basicCredential =
                        new NetworkCredential("jack@thesystemisdown.io", "MegurineLuka01!");
                    MailMessage message = new MailMessage();
                    MailAddress fromAddress = new MailAddress("alerts@thesystemisdown.io");

                    smtpClient.Host = "mail.thesystemisdown.io";
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = basicCredential;

                    message.From = fromAddress;
                    message.Subject = "HealthChkTool Alert";
                    //Set IsBodyHtml to true means you can send HTML email.
                    message.IsBodyHtml = true;
                    message.Body = "<h1>HealthChkTool has reported it the server is not responding to pings and may be down.</h1>";
                    message.To.Add("jack@thesystemisdown.io");

                    try
                    {
                        smtpClient.Send(message);
                        Console.WriteLine("Success");
                    }
                    catch (Exception ex)
                    {
                        //Error, could not send the message
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.InnerException.Message);
                        Console.WriteLine("Email Failed");
                        Console.ReadLine();
                    }
                }
                Thread.Sleep(1000); //sleep for 10 seconds so we dont spam the console and make pings without flooding.
                //NOTE - Could also use this to hold a value for user to specify the amount of time between 'tests'.
            }

        }
    }


    //Todo. 
    //This should check an array of results and if there's an error for say... 5 times we jump out the loop and execute this lot.
    public class Mail
    {

    }
}
