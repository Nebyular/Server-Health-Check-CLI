using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Mail;

namespace HealthChkTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string targetIp = "8.8.8.8";
            string icmpRslts;

            Ping PingSender = new Ping();
            PingReply reply = PingSender.Send(targetIp);
        }
    }

    public class Smtp
    {

    }
}
