using System;
using System.Linq;
using System.Threading.Tasks;
using ASF.Core.Common.Helpers;
using ASF.Client.MS.PushConsumer.Interfaces;
using PushTechnology.ClientInterface.Client.Factories;
using PushTechnology.ClientInterface.Client.Features.Control.Topics;
using PushTechnology.ClientInterface.Client.Session;
using PushTechnology.ClientInterface.Client.Topics;
using PushTechnology.ClientInterface.Client.Topics.Details;
using PushTechnology.ClientInterface.Data.JSON;
using Newtonsoft.Json.Linq;
using ASF.Client.MS.PushConsumer;
using ASF.Core.Common.Cache.Interfaces;
using Confluent.Kafka;

namespace ASF.Client.MS.PushConsumer.Helpers
{
    public class TopicHelper : ITopicHelper, IDisposable
    {
        private IMemoryCache _memoryCache;
        private readonly IProducer<Null, string> _producer; //kafka

        public TopicHelper(IMemoryCache memoryCache, IProducer<Null, string> producer)
        {
            _memoryCache = memoryCache;
            _producer = producer;
        }
        public async Task<string[]> GetTopics(ISession session, string selector = "?^(?!.*(Views)).*//")
        {
            //check active connection
            if (session.State != SessionState.CONNECTED_ACTIVE)
                throw new ApplicationException("Connection to Diffusion is closed.");

            //match available topics
            var topics = await session.Topics.FetchRequest.FetchAsync(selector);

            return topics.Results.Select(t => t.Path).ToArray();
        }

        public void AddPTStream(ISession session, string topicpath)
        {
            session.Topics.AddStream(topicpath, new PushTechnologyValueStream(_memoryCache, _producer));
        }

        public async void SubscribeTopic(ISession session, string topic)
        {
            //check active connection
            if (session.State != SessionState.CONNECTED_ACTIVE)
                throw new ApplicationException("Connection to Diffusion is closed.");

            object topicSub = await session.Topics.SubscribeAsync(topic);
        }

        public async Task<bool> Exists(ISession session, string topic)
        {
            var topics = await GetTopics(session);

            return topics.Contains(topic);
        }

        /// <summary>
        /// Create a new topic on a Diffusion instance
        /// </summary>
        /// <param name="session">Active Diffusion Session</param>
        /// <param name="topic">Topic name</param>
        /// <param name="disableDeltaStreaming">disable delta streaming, publishing a calculated difference between previous and current message, there is a performance hit for calculating deltas</param>
        /// <param name="removeAt">optional explicit topic remove date</param>
        /// <param name="removeAfterInactivity">optional period (timespan) to remove topic after inactivity - must be more than 1 hour</param>
        /// <returns></returns>
        public Task<AddTopicResult> CreateTopic(ISession session, string topic, bool disableDeltaStreaming = true, DateTime? removeAt = null, TimeSpan? removeAfterInactivity = null)
        {
            var topicSpecification = session.TopicControl.NewSpecification(TopicType.JSON)
                .WithProperty(TopicSpecificationProperty.PublishValuesOnly, disableDeltaStreaming.ToString());

            //remove the topic after a specified date/time
            if (removeAt.HasValue && removeAt.Value > DateTime.Now)
            {
                topicSpecification = topicSpecification.WithProperty(TopicSpecificationProperty.Removal, $"when time after \"{removeAt.Value.ToUniversalTime().ToString("R")}\"");
            }

            //remove the topic after a period of inactivity
            if (removeAfterInactivity.HasValue && Math.Floor(removeAfterInactivity.Value.TotalHours) > 0)
            {
                topicSpecification = topicSpecification.WithProperty(TopicSpecificationProperty.Removal, $"when no updates for 15m after {Math.Floor(removeAfterInactivity.Value.TotalHours)}h");
            }

            return session.TopicControl.AddTopicAsync(topic, topicSpecification);
        }

        public Task<ITopicRemovalResult> RemoveTopic(ISession session, string topic)
        {
            return session.TopicControl.RemoveTopicsAsync(topic);
        }

        /// <summary>
        /// Publish a message onto a topic
        /// </summary>
        public Task<object> UpdateTopic(ISession session, string topic, IJSON value)
        {
            return session.TopicUpdate.SetAsync(topic, value);
        }

        public async Task ReCalculateTopicOfTopics(string topic, params string[] uris)
        {
            var ch = new ConnectionHelper();

            //for each uri - open connection, get available topics, publish a new ToT message
            foreach (var uri in uris)
            {
                //open connection
                var session = ch.OpenConnection(uri);

                //get available topics 
                var topics = await GetTopics(session);

                //create ToT message
                var value = CreateTopicOfTopicsMessage(topics);

                //create ToT topic if doesn't exist
                if (!topics.Contains(topic))
                {
                    await CreateTopic(session, topic);
                }

                //publish a new Tot message
                await UpdateTopic(session, topic, value);
            }
        }

        public IJSON CreateTopicOfTopicsMessage(string[] topics)
        {
            //serialize message
            var doc = new JObject
            {
                { "id", Guid.NewGuid() },
                { "topics", JToken.FromObject(topics) },
                { "created", DateTime.Now }
            };

            var json = JsonSerializer.ToString(doc);

            var value = Diffusion.DataTypes.JSON.FromJSONString(json);

            return value;
        }

        public void Dispose() { }

     
    }
}
