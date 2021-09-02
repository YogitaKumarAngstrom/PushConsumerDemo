using ASF.Client.MS.PushConsumer;
using PushTechnology.ClientInterface.Client.Callbacks;
using PushTechnology.ClientInterface.Client.Features;
using PushTechnology.ClientInterface.Client.Features.Topics;
using PushTechnology.ClientInterface.Client.Topics.Details;
using PushTechnology.ClientInterface.Data.JSON;
using System;
using log4net;
using ASF.Core.Common.Helpers.Interfaces;
using ASF.Core.Common.Helpers;
using ASF.Core.Common.Cache.Interfaces;
using Confluent.Kafka;

namespace ASF.Client.MS.PushConsumer
{
    public class PushTechnologyValueStream : IValueStream<IJSON>
    {

        private ILog _logger = LogManager.GetLogger(typeof(PushTechnologyValueStream));
        private readonly IMemoryCache _memoryCache;
        private readonly IProducer<Null, string> _producer; //kafka
        public PushTechnologyValueStream(IMemoryCache memoryCache, IProducer<Null, string> producer)
        {
            _memoryCache = memoryCache;
            _producer = producer;
        }

        public void OnClose()
        {
            //throw new NotImplementedException();
        }

        public void OnError(ErrorReason errorReason)
        {
            //throw new NotImplementedException();
        }

        public void OnSubscription(string topicPath, ITopicSpecification specification)
        {
            Console.WriteLine($"Subscribed to: {topicPath}");
            //throw new NotImplementedException();
        }

        public void OnUnsubscription(string topicPath, ITopicSpecification specification, TopicUnsubscribeReason reason)
        {
            Console.WriteLine($"Unsubscribed from: {topicPath}");
            //throw new NotImplementedException();
        }

        public void OnValue(string topicPath, ITopicSpecification specification, IJSON oldValue, IJSON newValue)
        {
            Console.WriteLine($"{topicPath}: {newValue.ToJSONString()}");

            MessageProcessor messageProcessor = new MessageProcessor(_memoryCache, _producer);
            messageProcessor.ProcessMessage(newValue);
        }
    }
}
