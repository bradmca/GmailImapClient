using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace GmailImapClient
{


  class Program
  {

    public static void Main(string[] args)
    {
      Console.BufferHeight = 2000;
      Console.ForegroundColor = ConsoleColor.Green;
      
      using (var client = new ImapClient())
      {

        var credentials = new NetworkCredential("your@email.com", "youremailpasword");
        var uri = new Uri("imaps://imap.gmail.com");

        using (var cancel = new CancellationTokenSource())
        {
          client.Connect(uri, cancel.Token);

          // Note: since we don't have an OAuth2 token, disable
          // the XOAUTH2 authentication mechanism.
          client.AuthenticationMechanisms.Remove("XOAUTH");

          client.Authenticate(credentials, cancel.Token);

          // The Inbox folder is always available on all IMAP servers...
          var inbox = client.Inbox;
          var trash = client.GetFolder(SpecialFolder.Trash);

          inbox.Open(FolderAccess.ReadWrite, cancel.Token);

          Console.WriteLine("Total messages: {0}", inbox.Count);

          var query = SearchQuery.DeliveredAfter(DateTime.Parse("2013-01-12"));

          foreach (var uid in inbox.Search(query, cancel.Token))
          {
            Console.WriteLine("------------------------------------------");
            var delete = false;
            var message = inbox.GetMessage(uid, cancel.Token);
            Console.WriteLine("{0} : {1} : {2} : {3}", DateTime.Now.ToShortTimeString(), uid, message.Subject, message.Date);

            var body = message.BodyParts.OfType<TextPart>().FirstOrDefault();

            try
            {
              if (body.Text == "")
              {
                //Html mail, get from another BodyPart
                body = message.BodyParts.OfType<TextPart>().Last();
              }
            }
            catch (Exception exception)
            {
              
            }
            
            if (body != null)
            {
              if (body.Text != "")
              {

                //Find suitable confirmation links and "click" them
                Regex linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                foreach (Match m in linkParser.Matches(body.Text))
                {
                  if ((m.Value.IndexOf("onf", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("ctivate", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("?ACT=", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("verify", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("signup", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("register", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("becomeamember", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("actkey=", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("passwordreset", StringComparison.OrdinalIgnoreCase) >= 0) ||
                      (m.Value.IndexOf("validat", StringComparison.OrdinalIgnoreCase) >= 0))
                    
                  {
                    ////Negative options
                    if ((m.Value.IndexOf("invalidate", StringComparison.OrdinalIgnoreCase) == -1))
                    {
                      Console.WriteLine(m.Value);
                      GetPage(m.Value);
                      delete = true;
                    }
                  }

                    //Get rid of shit
                  if ((body.Text.IndexOf("unsubscribe", StringComparison.OrdinalIgnoreCase) >= 0))
                  {
                      Console.WriteLine("DELETING RUBBISH");
                      delete = true;
                  }
                }

              }
            }

            if (delete == true)
            {

              inbox.AddFlags(new UniqueId[] { uid }, MessageFlags.Deleted, true, cancel.Token);
              inbox.Expunge(cancel.Token);
              Console.WriteLine("Deleted");
              Console.WriteLine();

            }

          }

          //Console.ReadLine();

          client.Disconnect(true, cancel.Token);
        }
      }
    }

    static void GetPage(string url)
    {
      // Create web client simulating IE6.
      using (WebClient client = new WebClient())
      {
        client.Headers["User-Agent"] =
        "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
        "(compatible; MSIE 6.0; Windows NT 5.1; " +
        ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        // Download data.
        try
        {
          //byte[] arr = client.DownloadData(url);
          client.DownloadDataCompleted += DownloadDataCompleted;
          client.DownloadDataAsync(new Uri(url));

          //Console.WriteLine(arr.Length);
        }
        catch (Exception exception)
        {
          Console.WriteLine(exception.Message);
        }

      }
    }

    static void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
    {
      try
      {
        byte[] raw = e.Result;
        Console.WriteLine(raw.Length + " bytes received");
      }
      catch (Exception exception)
      {

        Console.WriteLine(exception.Message);
      }

    }

  }
}