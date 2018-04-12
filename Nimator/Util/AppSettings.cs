using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Nimator.Util
{
    public interface IAppSettings
    {
        string CouchBaseUsername { get; }
        string CouchBasePassword { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IDictionary<string, string> _appSettings;
        private string _couchBaseUsername;
        private string _couchBasePassword;

        public string CouchBaseUsername => _couchBaseUsername ?? (_couchBaseUsername = _appSettings["couchbase-username"]);
        public string CouchBasePassword => _couchBasePassword ?? (_couchBasePassword = _appSettings["couchbase-password"]);

        public AppSettings([NotNull]NameValueCollection appSettings)
            : this(Guard.AgainstNull_Return(nameof(appSettings), appSettings).ToDictionary()) { }

        public AppSettings([NotNull]IDictionary<string, string> appSettings)
        {
            Guard.AgainstNull(nameof(appSettings), appSettings);
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
