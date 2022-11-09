using System.Net;
using System.Net.Mail;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

static string GetIP()
{
    string url = "http://checkip.dyndns.org";
    System.Net.WebRequest req = System.Net.WebRequest.Create(url);
    System.Net.WebResponse resp = req.GetResponse();
    System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
    string response = sr.ReadToEnd().Trim();
    string[] a = response.Split(':');
    string a2 = a[1].Substring(1);
    string[] a3 = a2.Split('<');
    string a4 = a3[0];

    return a4;
}

static string GetLatency(string IP)
{
    Ping ping = new Ping();
    PingReply reply = ping.Send(IP, 5000);

    return reply.RoundtripTime.ToString() + " ms";
}

static void SendMail(string from, string to, string pwd, long count)
{
    var fromAddr = new MailAddress(from);
    var toAddr = new MailAddress(to);

    string IP = GetIP();
    string latency = GetLatency(IP);

    string fromPwd = pwd;
    string subject = "IP for today (" + DateTime.Today.ToShortDateString() + ")";
    string body = "Current date and time is: <b>" + DateTime.Now + "</b><br />";
          body += "Your current IP is: <b>" + IP + "</b><br />";
          body += "Your latency to the destination address is: <b>" + latency + "</b><br /><br />";
          body += "This email has been sent automatically <b>" + count + "</b> times";

    var smtp = new SmtpClient
    {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromAddr.Address, fromPwd),
        Timeout = 20000
    };

    using (var msg = new MailMessage(fromAddr, toAddr)
    {
        Subject = subject,
        Body = body,
        IsBodyHtml = true
    })

    try
    {
        smtp.Send(msg);
        Console.WriteLine("Email sent successfully!");
    }
    catch (SmtpFailedRecipientException ex)
    {
        Console.WriteLine("There is an error during email transit: ");
        Console.WriteLine(ex.GetBaseException());
    }
}

static async void Start()
{
    EmailAddressAttribute e = new EmailAddressAttribute();
    string[] data = new string[4];

    string email;
    string destination;
    string password;
    long count = 1;

    string dir = $@"{Environment.GetEnvironmentVariable("appdata")}\Kips\Fetch IP";

    if (File.Exists($@"{dir}\emailconfig.cfg"))
    {
        data = LoadConfig();
        email = data[0];
        destination = data[1];
        password = data[2];
        count = data[3] != null ? Int64.Parse(data[3]) + 1 : 1;
    } else
    {
        Console.WriteLine("Enter your email address: ");
        email = Console.ReadLine();
        data[0] = email;

        Console.WriteLine("Enter your destination email address: ");
        destination = Console.ReadLine();
        data[1] = destination;

        Console.WriteLine("Enter your email provider password: ");
        password = Console.ReadLine();
        data[2] = password;
        data[3] = count.ToString();
    }

    Console.WriteLine("Sending email...");

    if (e.IsValid(email) && e.IsValid(destination) && !string.IsNullOrEmpty(password)) 
    {
        SendMail(email, destination, password, count);
        await SaveConfig(data);
    }
}

static async Task SaveConfig(string[] data)
{
    string dir = $@"{Environment.GetEnvironmentVariable("appdata")}\Kips\Fetch IP";
    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    await File.WriteAllLinesAsync($@"{dir}\emailconfig.cfg", data);
}

static string[] LoadConfig()
{
    string dir = $@"{Environment.GetEnvironmentVariable("appdata")}\Kips\Fetch IP";
    return File.ReadAllLines($@"{dir}\emailconfig.cfg");
}

Start();
