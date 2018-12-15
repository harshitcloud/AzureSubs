using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;

namespace TopicsAndSubscriptions
{
    class TopicsAndSubscriptionsConsole
    {
        static string TopicPath = "ordertopic";

        static NamespaceManager NamespaceMgr;
        static MessagingFactory Factory;
        static TopicClient OrderTopicclient;

        static void Main(string[] args)
        {


            CreateManagerAndFactory();
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.Write("Creating topics and subscriptions...");
            CreateTopicsAndSubscriptions();
            Console.WriteLine("Done!");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press enter to send messages");
            Console.ReadLine();

            // Create a TopicClient for ordertopic.
            OrderTopicclient = Factory.CreateTopicClient(TopicPath);

            Console.WriteLine("Sending orders...");


            // Send five orders with different properties.
            SendOrder(new Order()
            {
                Name = "Loyal Customer",
                Value = 19.99,
                Region = "USA",
                Items = 1,
                HasLoyltyCard = true
            });

            SendOrder(new Order()
            {
                Name = "Large Order",
                Value = 49.99,
                Region = "USA",
                Items = 50,
                HasLoyltyCard = false
            });

            SendOrder(new Order()
            {
                Name = "High Value Order",
                Value = 749.45,
                Region = "USA",
                Items = 45,
                HasLoyltyCard = false
            });

            SendOrder(new Order()
            {
                Name = "Loyal Europe Order",
                Value = 49.45,
                Region = "EU",
                Items = 3,
                HasLoyltyCard = true
            });

            SendOrder(new Order()
            {
                Name = "UK Order",
                Value = 49.45,
                Region = "UK",
                Items = 3,
                HasLoyltyCard = false
            });

            // Close the TopicClient.
            OrderTopicclient.Close();


            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press enter to receive messages");
            Console.ReadLine();

            // Receive all messages from the ordertopic subscriptions.
            ReceiveFromSubscriptions(TopicPath);

            // Close the MessagingFactory and all it created.
            Factory.Close();


            Console.ReadLine();
        }

        static void CreateManagerAndFactory()
        {
            // Retrieve the connection string.
            string connectionString =
                ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

            // Create the NamespaceManager
            NamespaceMgr = NamespaceManager.CreateFromConnectionString(connectionString);

            // Create the MessagingFactory
            Factory = MessagingFactory.CreateFromConnectionString(connectionString);


        }


        static void CreateTopicsAndSubscriptions()
        {
            // If the topic exists, delete it.
            if (NamespaceMgr.TopicExists(TopicPath))
            {
                NamespaceMgr.DeleteTopic(TopicPath);
            }

            // Create the topic.
            NamespaceMgr.CreateTopic(TopicPath);

            // Subscription for all orders
            NamespaceMgr.CreateSubscription
                (TopicPath, "allOrdersSubscription");


            // Subscriptions for USA and EU regions
            NamespaceMgr.CreateSubscription(TopicPath, "usaSubscription",
                new SqlFilter("Region = 'USA'"));
            NamespaceMgr.CreateSubscription(TopicPath, "euSubscription",
                new SqlFilter("Region = 'EU'"));
            NamespaceMgr.CreateSubscription(TopicPath, "euSubscription2",
                new SqlFilter("Region = 'eu'"));

            // Subscriptions for large orders, high value orders and loyal USA customers.
            NamespaceMgr.CreateSubscription(TopicPath, "largeOrderSubscription",
                new SqlFilter("Items > 30"));
            NamespaceMgr.CreateSubscription(TopicPath, "highValueSubscription",
                new SqlFilter("Value > 500"));
            NamespaceMgr.CreateSubscription(TopicPath, "loyaltySubscription",
                new SqlFilter("Loyalty = true AND Region = 'USA'"));

            // Correlation subscription for UK orders.
            NamespaceMgr.CreateSubscription(TopicPath, "ukSubscription",
                new CorrelationFilter("UK"));

        }




        static void SendOrder(Order order)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Sending {0}...", order.Name);

            // Create a message from the order.
            BrokeredMessage orderMsg = new BrokeredMessage(order);

            // Promote properties.
            orderMsg.Properties.Add("Loyalty", order.HasLoyltyCard);
            orderMsg.Properties.Add("Items", order.Items);
            orderMsg.Properties.Add("Value", order.Value);
            orderMsg.Properties.Add("Region", order.Region);

            // Set the CorrelationId to the region.
            orderMsg.CorrelationId = order.Region;

            // Send the message.
            OrderTopicclient.Send(orderMsg);

            Console.WriteLine("Done!");
        }

        private static void ReceiveFromSubscriptions(string topicPath)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Receiving from topic {0} subscriptions.", topicPath);

            // Loop through the subscriptions in a topic.
            foreach (SubscriptionDescription subDescription in
                NamespaceMgr.GetSubscriptions(topicPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Receiving from subscription {0}...", subDescription.Name);

                // Create a SubscriptionClient
                SubscriptionClient subClient =
                    Factory.CreateSubscriptionClient(topicPath, subDescription.Name);



                // Receive all the massages form the subscription.
                Console.ForegroundColor = ConsoleColor.Green;
                while (true)
                {
                    // Recieve any message with a one second timeout.
                    BrokeredMessage msg = subClient.Receive(TimeSpan.FromSeconds(1));
                    if (msg != null)
                    {
                        // Deserialize the message body to an order.
                        Order order = msg.GetBody<Order>();
                        //Console.WriteLine("    Received {0}", order.Name);
                        Console.WriteLine("    Name {0} {1} items {2} ${3} {4}",
                            order.Name, order.Region, order.Items, order.Value,
                            order.HasLoyltyCard ? "Loyal" : "Not loyal");

                        // Mark the message as complete.
                        msg.Complete();
                    }
                    else
                    {
                        Console.WriteLine();
                        break;
                    }
                }

                // Close the SubscriptionClient.
                subClient.Close();
            }
            Console.ResetColor();
        }


    }
}
