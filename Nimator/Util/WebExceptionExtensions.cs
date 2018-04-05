using System.Net;

namespace Nimator.Util
{
    internal static class WebExceptionExtensions
    {
        public static string GetHttpStatus(this WebException exception)
        {
            Guard.AgainstNull(nameof(exception), exception);

            if (exception.Status == WebExceptionStatus.ProtocolError)
            {
                if (exception.Response is HttpWebResponse response)
                {
                    return (int)response.StatusCode + " " + response.StatusCode;
                }
            }

            return "HttpStatus Not Available";
        }
    }
}
