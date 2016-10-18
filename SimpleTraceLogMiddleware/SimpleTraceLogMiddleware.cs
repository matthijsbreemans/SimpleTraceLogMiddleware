using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Owin;

namespace SimpleTraceLogMiddleware
{
    /// <summary>
    /// Extreme simple webapi response trace logger
    /// </summary>
    public class SimpleTraceLogMiddleware
    {
        /// <summary>
        /// Next method function
        /// </summary>
        private readonly Func<IDictionary<string, object>, Task> nextFunc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nextFunc">Next function to invoke</param>
        public SimpleTraceLogMiddleware(Func<IDictionary<string, object>, Task> nextFunc)
        {
            this.nextFunc = nextFunc;
        }

        /// <summary>
        /// This is where it happens
        /// </summary>
        /// <param name="env">Environment parameters</param>
        /// <returns></returns>
        public async Task Invoke(IDictionary<string, object> env)
        {
            //First, call the next function
            await nextFunc.Invoke(env);

            //Parse response
            var responseStatusCode = tryGetFromDictionary(env, "ResponseStatusCode");
            var responseReasonPhrase = tryGetFromDictionary(env, "ResponseReasonPhrase");
            var requestPath = tryGetFromDictionary(env, "RequestPath");
            var requestMethod = tryGetFromDictionary(env, "RequestMethod");
            var requestQueryString = tryGetFromDictionary(env, "RequestQueryString");

            //quick way to fix OWIN's header's mismatch
            if (!string.IsNullOrWhiteSpace(responseStatusCode))
            {
                var fullpath = requestMethod + " " + requestPath +
                               (string.IsNullOrWhiteSpace(requestQueryString) ? string.Empty : "?" + requestQueryString);
                if (responseStatusCode.StartsWith("5"))
                {
                    Trace.TraceError("[ERROR " + fullpath + " ] " + responseStatusCode + " - " + responseReasonPhrase);
                }
                else if (responseStatusCode.ToString().StartsWith("4") || responseStatusCode.ToString().StartsWith("4"))
                {
                    Trace.TraceWarning("[WARN " + fullpath + " ] " + responseStatusCode + " - " + responseReasonPhrase);
                }
                else
                {
                    Trace.TraceInformation("[INFO " + fullpath + " ] " + responseStatusCode + " - " + responseReasonPhrase);
                }

            }

        }

        private string tryGetFromDictionary(IDictionary<string, object> dictionary, string match)
        {
            var key = dictionary.Keys.FirstOrDefault(x => x.EndsWith(match));
            return (key != null ? dictionary[key].ToString() : string.Empty);
        }
    }
    /// <summary>
    /// Static class for the middleware extension
    /// </summary>
    public static class SimpleTraceLogMiddlewareExtension
    {
        /// <summary>
        /// Extension method
        /// </summary>
        /// <param name="app">Owin app builder</param>
        /// <returns>App builder</returns>
        public static IAppBuilder UseSimpleTraceLogMiddleware(this IAppBuilder app)
        {
            return app.Use(typeof(SimpleTraceLogMiddleware));
        }
    }
}
