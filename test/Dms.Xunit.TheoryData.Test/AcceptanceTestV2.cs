using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Dms.Xunit.TheoryData.Test
{
    // Stolen from xunit test.utility
    public class AcceptanceTestV2 : IDisposable
    {
        protected Xunit2 Xunit2 { get; private set; }

        public void Dispose()
        {
            if (Xunit2 != null)
                Xunit2.Dispose();
        }

        public List<IMessageSinkMessage> Run(Type type)
        {
            return Run(new[] { type });
        }

        public List<IMessageSinkMessage> Run(Type[] types)
        {
            Xunit2 = new Xunit2(AppDomainSupport.IfAvailable, new NullSourceInformationProvider(), types[0].Assembly.CodeBase, configFileName: null, shadowCopy: true);

            var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();
            foreach (var type in types)
            {
                Xunit2.Find(type.FullName, includeSourceInformation: false, messageSink: discoverySink, discoveryOptions: TestFrameworkOptions.ForDiscovery());
                discoverySink.Finished.WaitOne();
                discoverySink.Finished.Reset();
            }

            var testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(msg => msg.TestCase).ToArray();

            var runSink = new SpyMessageSink<ITestAssemblyFinished>();
            Xunit2.RunTests(testCases, runSink, TestFrameworkOptions.ForExecution());
            runSink.Finished.WaitOne();

            return runSink.Messages.ToList();
        }

        public List<TMessageType> Run<TMessageType>(Type type)
            where TMessageType : IMessageSinkMessage
        {
            return Run(type).OfType<TMessageType>().ToList();
        }

        public List<TMessageType> Run<TMessageType>(Type[] types)
            where TMessageType : IMessageSinkMessage
        {
            return Run(types).OfType<TMessageType>().ToList();
        }
    }

    public class SpyMessageSink<TFinalMessage> : LongLivedMarshalByRefObject, IMessageSink
    {
        readonly Func<IMessageSinkMessage, bool> cancellationThunk;

        public SpyMessageSink(Func<IMessageSinkMessage, bool> cancellationThunk = null)
        {
            this.cancellationThunk = cancellationThunk ?? (msg => true);
        }

        public ManualResetEvent Finished = new ManualResetEvent(initialState: false);

        public List<IMessageSinkMessage> Messages = new List<IMessageSinkMessage>();

        /// <inheritdoc/>
        public void Dispose()
        {
            Finished.Dispose();
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            Messages.Add(message);

            if (message is TFinalMessage)
                Finished.Set();

            return cancellationThunk(message);
        }
    }

}