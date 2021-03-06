using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Thon.Hotels.FishBus.Options;

namespace Thon.Hotels.FishBus
{
    public class MessagingConfiguration
    {
        public IEnumerable<MessageDispatcher> Dispatchers { get; private set; }

        public MessagingConfiguration(IOptions<MessageSources> messageSources, MessageHandlerRegistry registry, IServiceScopeFactory scopeFactory, LogCorrelationOptions logCorrelationOptions)
        {
            SubscriptionClient CreateSubscriptionClient(Subscription s) =>
                new SubscriptionClient(new ServiceBusConnectionStringBuilder(s.ConnectionString), s.Name);

            QueueClient CreateQueueClient(Queue q) =>
                new QueueClient(new ServiceBusConnectionStringBuilder(q.ConnectionString));

            Dispatchers = messageSources
                .Value
                .Subscriptions
                .Select(subscription => new MessageDispatcher(scopeFactory, CreateSubscriptionClient(subscription), registry, logCorrelationOptions))
                .Concat(
                    messageSources
                        .Value
                        .Queues
                        .Select(queue => new MessageDispatcher(scopeFactory, CreateQueueClient(queue), registry, logCorrelationOptions))
                )
                .ToList();
        }

        public void RegisterMessageHandlers(Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            Dispatchers
                .ToList()
                .ForEach(d => d.RegisterMessageHandler(exceptionReceivedHandler));
        }

        public async Task Close()
        {
            await Task.WhenAll(
                Dispatchers
                .Select(async d => await d.Close())
                .ToArray()
            );
        }
    }
}
