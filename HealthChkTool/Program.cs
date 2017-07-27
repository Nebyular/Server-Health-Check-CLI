using System;
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
            //Use our ConsoleCopy class which contains the loggings logic
            using (var cc = new ConsoleCopy("log.txt"))
            {
                //Some initial setting
                string time = DateTime.Now.ToString();
                var reader = new AppSettingsReader();

                try
                    //try to get all our values in the app.config file
                {
                    var targetIp = reader.GetValue("IpAddr", typeof(string));
                    Console.WriteLine(time + "\nApplication Settings");
                    Console.WriteLine("\nTarget server setting: " + targetIp);
                    targetIp.ToString();

                    var dataVal = reader.GetValue("DataString", typeof(string));
                    Console.WriteLine("Ping packet setting: " + dataVal);

                    var timeout = reader.GetValue("TimeoutPeriod", typeof(int));
                    Console.WriteLine("Ping timeout setting: " + timeout);

                    var fragment = reader.GetValue("DontFragment", typeof(bool));
                    Console.WriteLine("Don't fragment setting: " + fragment);

                    var thresholdVal = reader.GetValue("AlertValue", typeof(int));
                    Console.WriteLine("Faliure threshold setting: " + thresholdVal);
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
                    Console.WriteLine("Email body setting: " + body + "\n");

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
                            Console.Write(time);
                            Console.WriteLine("\nAddress: {0}", reply.Address.ToString());
                            Console.WriteLine("     RoundTrip time: {0}", reply.RoundtripTime);
                            Console.WriteLine("     Time to live: {0}", reply.Options.Ttl);
                            Console.WriteLine("     Don't fragement: {0}", reply.Options.DontFragment);
                            Console.WriteLine("     Buffer size: {0}\n", reply.Buffer.Length);
                        }
                        else
                        {
                            Console.WriteLine(time + ": " + reply.Status);
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
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(time + ": [ERROR] " + ex.Message + " " + ex.InnerException.Message + ". Email Failed!\n");
                                Console.Write("Press any key to close...");
                                Console.ReadLine();
                            }
                        }
                        Thread.Sleep(sleepWait); //sleep for 10 seconds so we dont spam the console and make pings without flooding.
                    }
                }

                catch (InvalidOperationException e)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(time + ": [ERROR] Config error: " + e.Message);
                    Console.Write("Press any key to close...");
                    Console.ReadLine();
                }
            }

        }
    }
}
