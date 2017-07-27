using System;
using System.Text;
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
            using (var cc = new ConsoleCopy("log.txt"))
            {

                string time = DateTime.Now.ToString();
                var reader = new AppSettingsReader();

                var targetIp = reader.GetValue("IpAddr", typeof(string));
                Console.WriteLine(time + ": Target server setting: " + targetIp);
                targetIp.ToString();

                var dataVal = reader.GetValue("DataString", typeof(string));
                Console.WriteLine(time + ": Ping packet setting: " + dataVal);

                var timeout = reader.GetValue("TimeoutPeriod", typeof(int));
                Console.WriteLine(time + ": Ping timeout setting: " + timeout);

                var fragment = reader.GetValue("DontFragment", typeof(bool));
                Console.WriteLine(time + ": Don't fragment setting: " + fragment);

                var thresholdVal = reader.GetValue("AlertValue", typeof(int));
                Console.WriteLine(time+": Faliure threshold setting: " + thresholdVal);
                int threshold = (int)thresholdVal;

                var sleepVal = reader.GetValue("WaitTime", typeof(int));
                Console.WriteLine(time + ": Ping wait time setting: " + sleepVal);
                int sleepWait = (int)sleepVal;

                var emailPort = reader.GetValue("SmtpPort", typeof(int));
                Console.WriteLine(time + ": SMTP port setting: " + emailPort);
                int port = (int)emailPort;

                var host = reader.GetValue("SmtpHost", typeof(string));
                Console.WriteLine(time + ": SMTP host setting: " + host);

                var username = reader.GetValue("AuthUsername", typeof(string));
                Console.WriteLine(time + ": SMTP username setting: " + username);

                var userPassword = reader.GetValue("AuthUserPassword", typeof(string));
                Console.WriteLine(time + ": SMTP password setting: " + userPassword);

                var from = reader.GetValue("FromAddress", typeof(string));
                Console.WriteLine(time + ": Email from address setting: " + from);

                var toAddress = reader.GetValue("ToAddress", typeof(string));
                Console.WriteLine(time + ": Email to setting: " + toAddress);

                var subject = reader.GetValue("Subject", typeof(string));
                Console.WriteLine(time + ": Email subject setting: " + subject);

                var body = reader.GetValue("Body", typeof(string));
                Console.WriteLine(time + ": Email body setting: " + body + "\n");

                try
                {
                    // var missingSetting = reader.GetValue("Int setting", typeof(Int32));
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(time + ": Missing key error: " + e.Message);
                }

                Console.WriteLine("Press any key to continue\n");
                //Console.ReadKey();

                int val = 0; //set initial value for loop

                Ping PingSender = new Ping();

                //Ping options -  controlled via app.config
                PingOptions options = new PingOptions();
                options.DontFragment = Convert.ToBoolean(fragment); //make our fragment indicator a bool again
                string data = (string)dataVal; //data for byte buffer (32 bytes normally)
                byte[] buffer = Encoding.ASCII.GetBytes(data); //set our byte buffer and ensure its ASCII encoded

                while (val != threshold)
                {
                    PingReply reply = PingSender.Send(targetIp.ToString(), Convert.ToInt32(timeout), buffer, options);

                    if (reply.Status == IPStatus.Success)
                    {
                        //Outputs to console
                        Console.WriteLine(time + ": Address: {0}", reply.Address.ToString());
                        Console.WriteLine(time + ": RoundTrip time: {0}", reply.RoundtripTime);
                        Console.WriteLine(time + ": Time to live: {0}", reply.Options.Ttl);
                        Console.WriteLine(time + ": Don't fragement: {0}", reply.Options.DontFragment);
                        Console.WriteLine(time + ": Buffer size: {0}", reply.Buffer.Length);
                    }
                    else
                    {
                        Console.WriteLine(time +": "+reply.Status);
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
                            Console.WriteLine("[ERROR] " + time + ": " + ex.Message + " " + ex.InnerException.Message + ". Email Failed!\n");
                            Console.ReadLine();
                        }
                    }
                    Thread.Sleep(sleepWait); //sleep for 10 seconds so we dont spam the console and make pings without flooding.
                                             //NOTE - Could also use this to hold a value for user to specify the amount of time between 'tests'.
                }
            }
        }
    }
}
