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
            object responseStatusCode;
            object responseReasonPhrase;

            //quick way to fix OWIN's header's mismatch
            if (env.TryGetValue(env.Keys.First(x => x.Contains("ResponseStatusCode")), out responseStatusCode) &&
                env.TryGetValue(env.Keys.First(x => x.Contains("ResponseReasonPhrase")), out responseReasonPhrase))
            {
                if (responseStatusCode != null)
                {
                    if (responseStatusCode.ToString().StartsWith("5"))
                    {
                        Trace.TraceError("ERROR " + responseStatusCode + " - " + responseReasonPhrase);
                    }
                    else if (responseStatusCode.ToString().StartsWith("4") || responseStatusCode.ToString().StartsWith("4"))
                    {
                        Trace.TraceWarning("WARN " + responseStatusCode + " - " + responseReasonPhrase);
                    }
                    else
                    {
                        Trace.TraceInformation("INFO " + responseStatusCode + " - " + responseReasonPhrase);
                    }

                }
            }

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
