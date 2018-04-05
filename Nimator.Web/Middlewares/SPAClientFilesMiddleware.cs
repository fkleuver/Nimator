using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.Web.Middlewares
{
 public sealed class SPAClientFilesOptions
    {
        public string BaseDirectory { get; set; }
        public string DefaultFile { get; set; } = "index.html";
        public ILog Logger { get; set; }
        public bool LogHandledRequests { get; set; }
        public bool LogSkippedRequests { get; set; }
        public bool AbortIfFileNotFound { get; set; }

        /// <summary>
        /// Request paths starting with these values (with or without leading slash) will be ignored so that they can be handled by WebAPI
        /// </summary>
        private string[] _apiRootPaths;
        public string[] ApiRootPaths
        {
            get => _apiRootPaths;
            set
            {
                _apiRootPaths = value;
                UpdateRegex();
            }
        }

        /// <summary>
        /// Request paths containing these values (with or without leading slash) but NOT starting with these values will have the preceding path parts removed
        /// This is needed when the SPA router is configured to use PushState
        /// </summary>
        private string[] _spaRootPaths;
        public string[] SPARootPaths
        {
            get => _spaRootPaths;
            set
            {
                _spaRootPaths = value;
                UpdateRegex();
            }
        }

        internal Regex ApiRootPathRegex { get; private set; }
        internal Regex SPARootPathRegex { get; private set; }
        internal Regex SPAStartsWithRootPathRegex { get; private set; }

        private void UpdateRegex()
        {
            if (ApiRootPaths != null)
            {
                ApiRootPathRegex = new Regex($"^/({string.Join("|", ApiRootPaths)})/", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            }
            if (SPARootPaths != null)
            {
                SPARootPathRegex = new Regex($"/({string.Join("|", SPARootPaths)})/", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                SPAStartsWithRootPathRegex = new Regex($"^/({string.Join("|", SPARootPaths)})/", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            }
        }

        public static SPAClientFilesOptions Default => new SPAClientFilesOptions();
    }

    public sealed class SPAClientFilesMiddleware : OwinMiddleware
    {
        private readonly SPAClientFilesOptions _options;
        private static readonly Regex SchemeRegex = new Regex("https?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public SPAClientFilesMiddleware(OwinMiddleware next, SPAClientFilesOptions options) : base(next)
        {
            Guard.AgainstNull(nameof(options), options);
            if (options.LogHandledRequests || options.LogSkippedRequests)
            {
                Guard.AgainstNull(nameof(options.Logger), options.Logger);
            }
            _options = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var method = context.Request.Method;
            var scheme = context.Request.Scheme;
            var requestpath = context.Request.Path.Value;

            if (method != "GET" || !SchemeRegex.IsMatch(scheme) || (_options.ApiRootPathRegex?.IsMatch(requestpath) ?? false))
            {
                if (_options.LogSkippedRequests)
                {
                    Log(() => $"Skipping request: {{ \"method\": \"{method}\", \"scheme\": \"{scheme}\", \"requestpath\": \"{requestpath}\" }}");
                }

                await Next.Invoke(context);
            }
            else
            {
                if (_options.LogHandledRequests)
                {
                    Log(() => $"Handling request: {{ \"method\": \"{method}\", \"scheme\": \"{scheme}\", \"requestpath\": \"{requestpath}\" }}");
                }

                string fullPath;
                var fullPathBuilder = new StringBuilder();
                fullPathBuilder.Append(_options.BaseDirectory);

                if (!requestpath.Contains("."))
                {
                    // Not a request for a file, so it must be a SPA PushState path; serve the default file
                    fullPathBuilder.Append($@"\{_options.DefaultFile}");
                }
                else
                {
                    if (_options.SPARootPathRegex == null)
                    {
                        // No Regex defined; serve the requested file path as-is
                        fullPathBuilder.Append(requestpath);
                    }
                    else if (_options.SPARootPathRegex.IsMatch(requestpath))
                    {
                        if (!_options.SPAStartsWithRootPathRegex.IsMatch(requestpath))
                        {
                            // The requested file path is prepended with the SPA PushState path; remove it
                            var pushStatePath = _options.SPARootPathRegex.Split(requestpath)[0];
                            fullPathBuilder.Append(requestpath.Substring(pushStatePath.Length));
                        }
                        else
                        {
                            // Path looks OK; serve the requested file path as-is
                            fullPathBuilder.Append(requestpath);
                        }
                    }
                    else if (requestpath.EndsWith(_options.DefaultFile))
                    {
                        // The requested file path is the default file and it's prepended with the SPA PushState path; simple serve the default file
                        fullPathBuilder.Append($@"\{_options.DefaultFile}");
                    }
                    else
                    {
                        // The requested file path is a file in the root directory prepended with the SPA PushState path; simply serve the file from the root
                        fullPathBuilder.Append($@"\{requestpath.Split('/').Last()}");
                    }
                }

                fullPath = fullPathBuilder.Replace("/", @"\").ToString();
                await ServeFile(context, fullPath);
            }
        }

        private async Task ServeFile(IOwinContext context, string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                if (_options.AbortIfFileNotFound)
                {
                    Log(() => $"File not found: {{ \"{nameof(fullPath)}\": \"{fullPath}\" }}. Aborting..");
                }
                else
                {
                    Log(() => $"File not found: {{ \"{nameof(fullPath)}\": \"{fullPath}\" }}. Proceeding..");
                    await Next.Invoke(context);
                }
            }
            else
            {
                var mime = MimeMapping.GetMimeMapping(fullPath);

                Log(() => $"Serving file: {{ \"{nameof(fullPath)}\": \"{fullPath}\", \"{nameof(mime)}\": \"{mime}\" }}");
                using (var file = File.OpenRead(fullPath))
                {
                    await file.CopyToAsync(context.Response.Body);
                }
                context.Response.Headers.Set("Content-Type", mime);
            }
        }

        private void Log(Func<string> messageFunc)
        {
            _options.Logger.Debug(() => $"[{nameof(SPAClientFilesMiddleware)}] {messageFunc()}");
        }
    }
}
