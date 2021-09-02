using System;
using System.Threading.Tasks;
using PushTechnology.ClientInterface.Client.Features.Control.Topics;
using PushTechnology.ClientInterface.Client.Session;
using PushTechnology.ClientInterface.Data.JSON;

namespace ASF.Client.MS.PushConsumer.Interfaces
{
    public interface ITopicHelper
    {
        Task<string[]> GetTopics(ISession session, string selector = "?^(?!.*(Views)).*//");
        void SubscribeTopic(ISession session, string topic);
        void AddPTStream(ISession session, string topicpath);
        Task<bool> Exists(ISession session, string topic);
        Task<AddTopicResult> CreateTopic(ISession session, string topic, bool disableDeltaStreaming = true, DateTime? removeAt = null, TimeSpan? removeAfterInactivity = null);
        Task<ITopicRemovalResult> RemoveTopic(ISession session, string topic);
        Task<object> UpdateTopic(ISession session, string topic, IJSON value);
        Task ReCalculateTopicOfTopics(string topic, params string[] uris);
        IJSON CreateTopicOfTopicsMessage(string[] topics);
        void Dispose();
    }
}