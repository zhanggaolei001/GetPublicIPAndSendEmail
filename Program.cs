using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using Newtonsoft.Json;
using RestSharp;

namespace GetMyIpAndSendEmailToMe
{
    class Program
    {
        private const string myipTxt = "MyIP.txt";
        private const string myEmail = "your@emal.com";
        private const string password = "your password";
        static void Main(string[] args)
        {
            var client = new RestClient("http://ip.360.cn/IPShare/info");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept", "application/json, text/plain, */*");
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36";
            request.AddHeader("Referer", "http://ip.360.cn/");
            request.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            string ip = "0.0.0.0";
            while (true)
            {
                try
                {
                    IRestResponse response = client.Execute(request);
                    var data = JsonConvert.DeserializeObject<IPModel>(response.Content);
                    ip = data.ip;
                    Console.WriteLine($"新IP为:{ip}");
                    if (!File.Exists(myipTxt))
                    {
                        SaveAndSendNewIp(ip);
                    }
                    else
                    {
                        var oldIp = File.ReadAllText(myipTxt);
                        Console.WriteLine($"上次IP为:{ip}");
                        if (oldIp != ip)
                        {
                            Console.WriteLine("IP发生变化,进行通知");
                            SaveAndSendNewIp(ip);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Mailbox(myEmail, e.Message);
                }
                Thread.Sleep(1000 * 60*60);//一小时一次
            }
        }

        private static void SaveAndSendNewIp(string ip)
        {
            File.WriteAllText(myipTxt, ip);
            Mailbox(myEmail, ip);
        }

        private static Boolean Mailbox(string name, string content)
        {
            MailMessage message = new MailMessage();
            //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
            System.Net.Mail.MailAddress fromAddr = new System.Net.Mail.MailAddress(myEmail, "我的IP");
            message.From = fromAddr;

            //设置收件人,可添加多个,添加方法与下面的一样
            message.To.Add(name);

            //设置邮件标题
            message.Subject = "我的IP";

            //设置邮件内容
            message.Body = content;

            //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.qq.com", 25)
                ;
            //设置发送人的邮箱账号和密码，POP3/SMTP服务要开启, 密码要是POP3/SMTP等服务的授权码

            client.Credentials = new System.Net.NetworkCredential(myEmail, password);//vtirsfsthwuadjfe  fhszmpegwoqnecja

            //启用ssl,也就是安全发送
            client.EnableSsl = true;

            //发送邮件
            client.Send(message);
            return true;
        }

    }
    public class IPModel
    {
        public string greetheader { get; set; }
        public string nickname { get; set; }
        public string ip { get; set; }
        public string location { get; set; }
        public string loc_client { get; set; }
    }
}
