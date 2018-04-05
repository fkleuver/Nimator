using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Nimator.Util
{
    public interface IAppSettings
    {
        string ClientBaseUri { get; }
        string ApiBaseUri { get; }
        string CouchBaseUsername { get; }
        string CouchBasePassword { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IDictionary<string, string> _appSettings;
        private string _clientBaseUri;
        private string _apiBaseUri;
        private string _couchBaseUsername;
        private string _couchBasePassword;
        
        public string ClientBaseUri => _clientBaseUri ?? (_clientBaseUri = _appSettings["client-base-uri"]);
        public string ApiBaseUri => _apiBaseUri ?? (_apiBaseUri = _appSettings["api-base-uri"]);
        public string CouchBaseUsername => _couchBaseUsername ?? (_couchBaseUsername = _appSettings["couchbase-username"]);
        public string CouchBasePassword => _couchBasePassword ?? (_couchBasePassword = _appSettings["couchbase-password"]);

        public AppSettings(NameValueCollection appSettings) : this(appSettings.ToDictionary())
        {
        }

        public AppSettings(IDictionary<string, string> appSettings)
        {
            _appSettings = appSettings;
        }

        public static AppSettings FromConfigurationManager()
        {
            return new AppSettings(ConfigurationManager.AppSettings);
        }
    }

    internal static class NameValueCollectionExtensions
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in col.AllKeys)
            {
                dict.Add(k, col[k]);
            }
            return dict;
        }
    }
}
