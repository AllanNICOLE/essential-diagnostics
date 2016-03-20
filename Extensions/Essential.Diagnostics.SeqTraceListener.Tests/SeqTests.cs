﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Essential.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Essential.Diagnostics.Tests.Utility;
using System.Net;

namespace Essential.Diagnostics.Tests
{
    [TestClass()]
    public class SeqTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SeqHandlesBasicEvent()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            var listener = new SeqTraceListener("http://testuri");
            listener.HttpWebRequestFactory = mockRequestFactory;

            listener.TraceEvent(null, "TestSource", TraceEventType.Warning, 1, "Test Message");

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual("application/json; charset=utf-8", request.ContentType);
            Assert.AreEqual("http://testuri/api/events/raw", request.Uri);
            Assert.AreEqual(0, request.Headers.Count);

            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "TestSource");
            StringAssert.Contains(requestBody, "Test Message");
        }

        [TestMethod]
        public void SeqHandlesEventFromTraceSource()
        {
            var mockRequestFactory = new MockHttpWebRequestFactory();
            mockRequestFactory.ResponseQueue.Enqueue(
                new MockHttpWebResponse(HttpStatusCode.OK, null)
                );

            TraceSource source = new TraceSource("seq1Source");
            var listener = source.Listeners.OfType<SeqTraceListener>().First();
            listener.HttpWebRequestFactory = mockRequestFactory;

            source.TraceEvent(TraceEventType.Warning, 2, "{0}-{1}", 3, "B");

            Assert.AreEqual(1, mockRequestFactory.RequestsCreated.Count);

            var request = mockRequestFactory.RequestsCreated[0];
            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual("application/json; charset=utf-8", request.ContentType);
            Assert.AreEqual("http://127.0.0.1:5341/api/events/raw", request.Uri);
            Assert.AreEqual(1, request.Headers.Count);

            var requestBody = request.RequestBody;
            Console.WriteLine(requestBody);
            StringAssert.Contains(requestBody, "seq1Source");
            StringAssert.Contains(requestBody, "{0}-{1}");
        }

        [TestMethod]
        public void SeqConfigParametersLoadedCorrectly()
        {
            TraceSource source = new TraceSource("seq2Source");
            var listener = source.Listeners.OfType<SeqTraceListener>().First();

            Assert.AreEqual("seq2", listener.Name);
            Assert.AreEqual("http://localhost:5341", listener.ServerUrl);
            Assert.AreEqual("12345", listener.ApiKey);

            Assert.AreEqual(TraceOptions.ThreadId, listener.TraceOutputOptions & TraceOptions.ThreadId);
        }

        // TODO: Test to check _all_ parameters work.

        // TODO: Test to check max message length trimming works.
    }
}