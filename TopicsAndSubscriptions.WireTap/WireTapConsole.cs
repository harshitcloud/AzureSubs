using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopicsAndSubscriptions.WireTap
{
    class WireTapConsole
    {
        //ToDo: Enter a valid Serivce Bus connection string
        static string SbConnectionString = "";
        static string TopicPath = "ordertopic";

        static void Main(string[] args)
        {
            Console.WriteLine("Hit enter to start wiretap");
            Console.ReadLine();

            var manager = NamespaceManager.CreateFromConnectionString
                (SbConnectionString);
            var subName = "wiretap-" + Guid.NewGuid().ToString();

            // Create a subscription that will expire
            manager.CreateSubscription(new SubscriptionDescription(TopicPath, subName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
                });


            var subClient = SubscriptionClient.CreateFromConnectionString
                (SbConnectionString, TopicPath, subName);

            // Receive messages and display properties
            subClient.OnMessage(message =>
            {
                Console.Write("Message received:");
                foreach (var item in message.Properties)
                {
                    Console.Write(" {0}={1}", item.Key, item.Value);
                }
                Console.WriteLine();
            });
            Console.WriteLine("Wire tap running...");
            Console.ReadLine();
        }
    }
}
