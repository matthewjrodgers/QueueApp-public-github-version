using System;
using System.Threading.Tasks;
// Azure Storage Client Library for .NET
// to install this package from a terminal run dotnet add package WindowsAzure.Storage
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace QueueApp
{
    class Program
    {

        //connection string. This string is no longer valid because it was using a MicrosoftLearn sandbox.
        private const string ConnectionString = "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=mslearnsa;AccountKey=YsvCmTzDOwBTJlMV1xHHiDfYqtYxgA/oLHUvcpmQHDaa2NXfyWPCqMjA2WtiK64T0phtWUcm2X7hKfQ+Rk+cBQ==";


        // this is how you connect to a queue
        static CloudQueue GetQueue()
        {
            // represents an Azure storage account. In this case it represents my particular storage account
            // because the connection string for it is passed to the class.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            // represents "Azure Queue storage"
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            // open a CloudQueue instance. This doesn't create the storage queue itself.
            // However, this object can be used to create, delete, and check for an existing queue.
            return queueClient.GetQueueReference("newsqueue");
        }

        // While the total queue size can be up to 500 TB, the individual messages in it can only be up to 64 KB in size (48 KB when using Base64 encoding). If you need a larger payload you can combine queues and blobs – passing the URL to the actual data (stored as a Blob) in the message. This approach would allow you to enqueue up to 200 GB for a single item.
        // this sends a message to the queue
        // best practice is only for the publisher to create the queue.
        static async Task SendArticleAsync(string newsMessage)
        {
            // create a CloudQueue object that we can use to work with the queue.
            CloudQueue queue = GetQueue();
            // create the queue if necessary, or return false if the queue already exists.
            // this ensures the queue is ready for use
            bool createdQueue = await queue.CreateIfNotExistsAsync();
            if (createdQueue)
            {
                Console.WriteLine("The queue of news articles was created.");
            }
            // represents a message
            CloudQueueMessage articleMessage = new CloudQueueMessage(newsMessage);
            // use the CloudQueue object to send the message to the queue
            await queue.AddMessageAsync(articleMessage);
        }

        // read next message in queue, process it, and delete it from the queue
        static async Task<string> ReceiveArticleAsync()
        {
            // get our object we can use to create, delete, and check for an existing queue.
            // this is called our queue reference
            CloudQueue queue = GetQueue();
            // check if the queue exists.
            // if we attempt to retrieve a message from a non-existent queue, the API will throw an exception
            bool exists = await queue.ExistsAsync();
            if (exists)
            {
                // represents a message
                // get the message. i.e. the next message in the queue
                // the return value will be null if the queue is empty
                CloudQueueMessage retrievedArticle = await queue.GetMessageAsync();
                if (retrievedArticle != null)
                {
                    // this is where processing would be performed
                    // this gets the contents of the message
                    string newsMessage = retrievedArticle.AsString;
                    // delete the message after processing completes
                    await queue.DeleteMessageAsync(retrievedArticle);
                    return newsMessage;
                }

            }
            return "<queue empty or not created>";
        }

        static void Main(string[] args)
        {
            // check the args param to see if any data was passed to the command line
            // if so we will send that message to the queue
            if (args.Length > 0)
            {
                // create a single string from all the words using a space as the separator
                string value = String.Join(" ", args);
                // pass args to the SendArticleAsync method
                SendArticleAsync(value).Wait();
                // output the message sent to the queue
                Console.WriteLine($"Sent: {value}");
            }
            else // we assume if you pass no params to the program, then you want the next message from the queue
            {
                string value = ReceiveArticleAsync().Result;
                Console.WriteLine($"Received {value}");
            }

        }
    }
}