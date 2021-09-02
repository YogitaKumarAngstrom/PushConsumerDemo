using System;
using ASF.Core.Common.Helpers;
using ASF.Client.MS.PushConsumer.Interfaces;
using PushTechnology.ClientInterface.Client.Session;
using PushTechnology.ClientInterface.Client.Factories;

namespace ASF.Client.MS.PushConsumer.Helpers
{
    public class ConnectionHelper : IConnectionHelper, IDisposable
    {
        public ISession OpenConnection(string uri)
        {
            //extract uri connection details
            var u = UriHelper.GetUriWithoutCredentials(uri);
            var (username, password) = UriHelper.GetUriCredentials(uri);

            return Diffusion.Sessions
                .Principal(username)
                .Password(password)
                .Open(u);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
