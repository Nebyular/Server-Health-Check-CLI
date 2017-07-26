using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Threading;
using System.Net;
using System.Configuration;

namespace HealthChkTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string time = DateTime.Now.ToString();
            var reader = new AppSettingsReader();

            var targetIp = reader.GetValue("IpAddr", typeof(string));
            Console.WriteLine("Target server setting: " + targetIp);
            targetIp.ToString();

            var dataVal = reader.GetValue("DataString", typeof(string));
            Console.WriteLine("Ping packet setting: " + dataVal);
           // data.ToString();

            var timeout = reader.GetValue("TimeoutPeriod", typeof(int));
            Console.WriteLine("Ping timeout setting: " + timeout);
            //Convert.ToInt32(timeout);

            var fragment = reader.GetValue("DontFragment", typeof(bool));
            Console.WriteLine("Don't fragment setting: " + fragment);
            //Convert.ToBoolean(fragment);

            var thresholdVal = reader.GetValue("AlertValue", typeof(int));
            Console.WriteLine("Faliure threshold setting: " + thresholdVal);
            //threshold = Convert.ToInt32(threshold);
            int threshold = (int)thresholdVal;

            var sleepVal = reader.GetValue("WaitTime", typeof(int));
            Console.WriteLine("Ping wait time setting: " + sleepVal);
            int sleepWait = (int)sleepVal;

            var emailPort = reader.GetValue("SmtpPort", typeof(int));
            Console.WriteLine("SMTP port setting: " + emailPort);
            int port = (int)emailPort;

            var host = reader.GetValue("SmtpHost", typeof(string));
            Console.WriteLine("SMTP host setting: " + host);

            var username = reader.GetValue("AuthUsername", typeof(string));
            Console.WriteLine("SMTP username setting: " + username);

            var userPassword = reader.GetValue("AuthUserPassword", typeof(string));
            Console.WriteLine("SMTP password setting: " + userPassword);

            var from = reader.GetValue("FromAddress", typeof(string));
            Console.WriteLine("Email from address setting: " + from);

            var toAddress = reader.GetValue("ToAddress", typeof(string));
            Console.WriteLine("Email to setting: " + toAddress);

            var subject = reader.GetValue("Subject", typeof(string));
            Console.WriteLine("Email subject setting: " + subject);

            var body = reader.GetValue("Body", typeof(string));
            Console.WriteLine("Email body setting: " + body+"\n");
            
            try
            {
               // var missingSetting = reader.GetValue("Int setting", typeof(Int32));
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Missing key error: " + e.Message);
            }

            Console.WriteLine("Press any key to continue\n");
            //Console.ReadKey();

            //string targetIp = AppSettingsReader[IPAddr];
            string icmpRslts; //maybe for logging use if this will capture values in an array
            int val = 0; //test value for loop

            Ping PingSender = new Ping();

            //Ping options -  tweak as needed
            PingOptions options = new PingOptions();
            options.DontFragment = Convert.ToBoolean(fragment);
            //string data = "12345678901234567890123456789012";
            string data = (string)dataVal;
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            //int timeout = 120;

            while (val != threshold)
            {
                PingReply reply = PingSender.Send(targetIp.ToString(), Convert.ToInt32(timeout), buffer, options);

                if (reply.Status == IPStatus.Success)
                {
                    //Outputs to console
                    Console.WriteLine("Address: {0}", reply.Address.ToString());
                    Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                    Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                    Console.WriteLine("Don't fragement: {0}", reply.Options.DontFragment);
                    Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                }
                else 
                {
                    Console.WriteLine(reply.Status);
                    val = (++val);
                }

                if (val == threshold)
                {
                    SmtpClient smtpClient = new SmtpClient();
                    NetworkCredential basicCredential =
                        new NetworkCredential(username.ToString(), userPassword.ToString());
                    MailMessage message = new MailMessage();
                    MailAddress fromAddress = new MailAddress(from.ToString());

                    smtpClient.Host = host.ToString();
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = basicCredential;
                    smtpClient.Port = port;

                    message.From = fromAddress;
                    message.Subject = subject.ToString();
                    //Set IsBodyHtml to true means you can send HTML email.
                    message.IsBodyHtml = true;
                    message.Body = body.ToString();
                    message.To.Add(toAddress.ToString());

                    try
                    {
                        smtpClient.Send(message);
                        Console.WriteLine(time + ": Success!\n");
                    }
                    catch (Exception ex)
                    {
                        //Error, could not send the message
                        Console.WriteLine("[ERROR] "+time+": "+ex.Message+" "+ex.InnerException.Message+". Email Failed!\n");
                        Console.ReadLine();
                    }
                }
                Thread.Sleep(sleepWait); //sleep for 10 seconds so we dont spam the console and make pings without flooding.
                //NOTE - Could also use this to hold a value for user to specify the amount of time between 'tests'.
            }

        }
    }


    //Todo. 
    //This should check an array of results and if there's an error for say... 5 times we jump out the loop and execute this lot.
}
