using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;

using SimpleTraceLogMiddleware;

namespace SimpleTraceLogMiddleware.Tests
{
    [TestClass]
    public class SimpleTraceLogMiddlewareTests
    {

        [TestMethod]
        public async Task OwinAppTest()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                var listener = new DebugTraceListener();

                //clear trace listeners and add our own
                Trace.Listeners.Clear();
                Trace.Listeners.Add(listener);

                //do request
                var response = await server.HttpClient.GetAsync("/");

                //check if our tracelistener has the correct value
                Assert.AreEqual<string>(listener.Value, "ERROR 500 - Internal server error");
            }
        }


        public class TestStartup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseSimpleTraceLogMiddleware();
                app.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ReasonPhrase = "Internal server error";
                    await context.Response.WriteAsync("Hello world using OWIN TestServer");

                });
                
            }
        }
    }

    public class DebugTraceListener : TraceListener
    {
        public string Value { get; set; }
        public DebugTraceListener()
        {
            this.Name = "Testing Trace Listener";
        }

        public DebugTraceListener(string name)
            : base(name)
        {
        }

        public override void Write(string message)
        {

        }

        public override void WriteLine(string message)
        {
            Value = message;
        }
        public override void Fail(string message, string detailMessage)
        {
        }
    }
}
