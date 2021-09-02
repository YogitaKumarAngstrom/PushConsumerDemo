using System;
using System.Threading.Tasks;
using ASF.Core.Common.Cache.Interfaces;
using ASF.Core.Common.Consts;
using ASF.Core.Common.Helpers.Interfaces;
using ASF.Client.MS.PushConsumer.Helpers;
using log4net;
using PushTechnology.ClientInterface.Client.Factories;
using PushTechnology.ClientInterface.Client.Session;
using PushTechnology.ClientInterface.Data.JSON;
using Confluent.Kafka;

namespace ASF.Client.MS.PushConsumer
{
    public class MessageProcessor
    {
        private ISerializer _serializer;
        private ILog _logger = LogManager.GetLogger(typeof(MessageProcessor));
        private IMemoryCache _memoryCache;
        private readonly IProducer<Null, string> _producer; //kafka
        public MessageProcessor(IMemoryCache memoryCache, IProducer<Null, string> producer)
        {
            _producer = producer;
            //open memory cache connection
            _memoryCache = memoryCache;
        }

        public void ProcessMessage(IJSON message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("s")} - Message: {message}");
            //_memoryCache.SetStringAsync(Topics.ADJUSTMENT_POC, message.ToJSONString(), null, StackExchange.Redis.When.Always, StackExchange.Redis.CommandFlags.None).Wait();

            //_producer.ProduceAsync(Topics.TEST_ADJUSTMENT_POC, new Message<Null, string> { Value = message.ToJSONString()  }).Wait();

            //var publishMessage = _serializer.Deserialize<Common.Messages.Messages.PublishMessage>(message);
        }
    }
}
