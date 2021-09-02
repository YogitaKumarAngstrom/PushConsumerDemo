using PushTechnology.ClientInterface.Client.Session;

namespace ASF.Client.MS.PushConsumer.Interfaces
{
    public interface IConnectionHelper
    {
        void Dispose();
        ISession OpenConnection(string uri);
    }
}