using ClientApp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    // List to store received broadcasts
    static List<string> receivedBroadcasts = new List<string>();
    // List to store private messages
    static List<string> privateTexts = new List<string>();
    static bool exit = true;
    static async Task Main(string[] args)
    {
        Person person = new Person();
        const int bufferSize = 1024 * 1024; // Around 2 Megabytes.
        bool wanttodosomething = true;
        string hostname = "192.168.10.2";
        TcpClient client = new TcpClient(hostname, 1234);
        int loopcounter = 0;
        string Clientname = null;
        string Clientnumber = null;
        int choice = -10;

        // Start a separate thread to listen for broadcasts
        Thread broadcastThread = new Thread(() => ListenForBroadcasts(client));

        while (exit == true)
        {
            try
            {
                if (wanttodosomething == true)
                {
                Choose:
                    await Task.Delay(1000);
                    Console.WriteLine();
                    Console.WriteLine("╔═══════════════════════════════════════╗");
                    Console.WriteLine("║                 Index                 ║");
                    Console.WriteLine("║---------------------------------------║");
                    Console.WriteLine("║  1. To Register.                      ║");
                    Console.WriteLine("║  2. To Broadcast Message.             ║");
                    Console.WriteLine("║  3. To Send Message to Specific User. ║");
                    Console.WriteLine("║  4. To Display Recieved Messages.     ║");
                    Console.WriteLine("║  5. To Display Private Chats          ║");
                    Console.WriteLine("║  6. Exit Program                      ║");
                    Console.WriteLine("╚═══════════════════════════════════════╝");
                    Console.WriteLine();

                    Console.Write("Enter Choice: ");
                    int.TryParse(Console.ReadLine(), out choice);
                    Console.WriteLine();

                    if (loopcounter == 1 && choice == 1)
                    {
                        Console.WriteLine("Client Already Registered, No Need To Register Again");
                        Console.WriteLine();
                        goto Choose;
                    }
                    if (loopcounter == 0 && choice != 1)
                    {
                        Console.WriteLine("Client Needs To Register For Communication");
                        Console.WriteLine();
                        goto Choose;
                    }

                    switch (choice)
                    {
                        case 1:

                            byte[] namebytes = new byte[bufferSize]; // For Sending Name.
                            byte[] phonebytes = new byte[bufferSize]; // For Sending Phone Number.
                            Console.Write("Enter your name: ");
                            Clientname = Console.ReadLine();

                            while (string.IsNullOrWhiteSpace(Clientname))
                            {
                                Console.Write("Enter your name (name cannot be null): ");
                                Clientname = Console.ReadLine();
                            }

                            await ClientClass.SendMessagetoServerAsync(Clientname, client);

                        PhoneSend:
                            Console.WriteLine("Enter your number: ");
                        input:
                            string input = Console.ReadLine();
                            if (!long.TryParse(input, out _))       // here i am just checking whther the string is a valid number or not the "_" sign means I am not wanting to store the parse value i am ignoring it, "_" is also called a disard sign.
                            {
                                Console.Write("Enter a valid number: ");
                                goto input;
                            }
                            Clientnumber = input;
                            await Task.Delay(1000); // 1 sec (1000 milliseconds) delay.
                                                    // added delay because when data sent simultaneously without delay then name and phone on server end comes as one (like this abdulmuqeet04124252) instead of coming separately
                            await ClientClass.SendMessagetoServerAsync(Clientnumber, client);
                            Console.WriteLine();
                            Console.WriteLine("=======================");
                            Console.WriteLine("Connected to server");
                            Console.WriteLine("=======================");
                            Console.WriteLine("Waiting for the response......");
                            Console.Write("Response: ");

                            // Server's Response
                            byte[] message_Bytes = new byte[bufferSize];
                            string response_Message = await ClientClass.ReceiveMessagefromServerAsync(message_Bytes, client);
                            Console.WriteLine(response_Message);
                            Console.WriteLine();
                            await Task.Delay(1000);
                            // If Error Comes server sends -1 and if not then 0
                            byte[] errorCodeBytes = new byte[bufferSize];
                            string responseErrorCode = await ClientClass.ReceiveMessagefromServerAsync(errorCodeBytes, client);

                            if (responseErrorCode == "-1")
                            {
                                Console.WriteLine("Re-enter Different Number:");
                                goto PhoneSend;
                            }
                            else
                            {
                                if (responseErrorCode == "0")
                                {
                                    person.setName(Clientname);
                                    person.setNumber(Clientnumber);
                                    loopcounter = 1;
                                }
                            }
                            broadcastThread.Start();
                            break;

                        case 2:
                            Console.WriteLine("╔════════════════════════════╗");
                            Console.WriteLine("║    Broadcasting Message    ║");
                            Console.WriteLine("╚════════════════════════════╝");
                            Console.WriteLine();
                            string MessagesendType = "Broadcast";
                            await ClientClass.SendMessagetoServerAsync(MessagesendType, client);
                            await Task.Delay(1000);
                            Console.Write("Enter Message To Broadcast: ");
                            string Messagesend = Console.ReadLine();
                            while (Messagesend == null || Messagesend == " ")
                            {
                                Console.Write("Enter your message (Message can't be empty): ");
                                Messagesend = Console.ReadLine();
                            }
                            DateTime time = DateTime.Now;
                            string finalMessage = "Sent on " + time + " : " + Clientname + "( " + person.getPhoneNumber() + " ) Says: " + Messagesend;
                            await ClientClass.SendMessagetoServerAsync(finalMessage, client);
                            wanttodosomething = true;
                            break;

                        case 3:
                            Console.WriteLine("╔════════════════════════════════════════╗");
                            Console.WriteLine("║    Sending Message to Desired Person   ║");
                            Console.WriteLine("╚════════════════════════════════════════╝");
                            Console.WriteLine();
                            string MessageType = "Private";
                            await ClientClass.SendMessagetoServerAsync(MessageType, client);
                            await Task.Delay(1000);
                            Console.Write("Enter Message To Send: ");
                            string Messagetosend = Console.ReadLine();
                            while (Messagetosend == null || Messagetosend == " ")
                            {
                                Console.Write("Enter your message (Message can't be empty): ");
                                Messagetosend = Console.ReadLine();
                            }

                            Console.Write("Enter The Number You Want This Message To Go To: ");
                            string DestinationNumber = Console.ReadLine();
                            while (DestinationNumber == null || DestinationNumber == " ")
                            {
                                Console.Write("Enter your message (Message can't be empty): ");
                                DestinationNumber = Console.ReadLine();
                            }
                            DateTime timep = DateTime.Now;
                            string finalMessageis = "Sent on " + timep + " : " + Clientname + "( " + Clientnumber + " ) Says: " + Messagetosend;
                            await ClientClass.SendMessagetoSpecificClientAsync(DestinationNumber, finalMessageis, client);
                            break;

                        case 4:
                            // Display received broadcasts
                            Console.WriteLine("Received Broadcasts:");
                            foreach (var broadcast in receivedBroadcasts)
                            {
                                Console.WriteLine(broadcast);

                                if (broadcast == null)
                                {
                                    Console.WriteLine("No New Private Chats");
                                }
                            }
                            goto Choose;

                        case 5:
                            // Display received broadcasts
                            Console.WriteLine("Received Private Chats:");
                            foreach (var privatemessages in privateTexts)
                            {
                                Console.WriteLine(privatemessages);
                                if (privatemessages == null)
                                {
                                    Console.WriteLine("No New Private Chats");
                                }
                            }
                            goto Choose;

                        default:
                            Console.WriteLine("Existing....!");
                            exit = false;
                            broadcastThread.Join();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Occurred: " + e.Message);
            }
        }
    }

    static async void ListenForBroadcasts(TcpClient client)
    {
        try
        {
            while (exit == true)
            {

                // Listen for broadcast messages
                byte[] buffer = new byte[1024 * 1024];
                string Mes_sage = await ClientClass.ReceiveMessagefromServerAsync(buffer, client);
                // Store received broadcasts
                if (Mes_sage == "Private")
                {

                    byte[] pbuffer = new byte[1024 * 1024];
                    string realMessage = await ClientClass.ReceiveMessagefromServerAsync(pbuffer, client);
                    privateTexts.Add(realMessage);
                }
                else
                {


                    byte[] bbuffer = new byte[1024 * 1024];
                    string broadcastMessage = await ClientClass.ReceiveMessagefromServerAsync(bbuffer, client);
                    receivedBroadcasts.Add(broadcastMessage);

                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error occurred while listening for broadcasts: " + e.Message);
        }
    }

}
