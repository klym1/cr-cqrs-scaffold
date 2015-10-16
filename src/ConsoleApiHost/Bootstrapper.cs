using System;
using System.Collections.Generic;
using CR.MessageDispatch.Core;
using CR.MessageDispatch.Dispatchers.EventStore;
using CR.ViewModels.Core;
using CR.ViewModels.Persistance.Memory;
using EventStore.ClientAPI;
using Nancy.TinyIoc;

namespace ConsoleApiHost
{
    public class Bootstrapper : RestApiBootstrapper
    {
        //TODO: Sort out this!

        protected override List<IDispatcher<ResolvedEvent>> GetDispatchers(TinyIoCContainer container)
        {
            //var batchRecordDenormalizer = new BatchRecordDenormalizer(container.Resolve<IViewModelWriter>());
            var typedRegistry = new MessageHandlerRegistry<Type>();

            //typedRegistry.AddByConvention(batchRecordDenormalizer);

            var aggregateDispatcher = new EventStoreAggregateEventDispatcher(typedRegistry);

            return new List<IDispatcher<ResolvedEvent>>() { aggregateDispatcher };
        }

        protected override IEnumerable<object> GetPairs(InMemoryViewModelRepository repo)
        {
            //foreach (var item in (IDictionary<string, BatchRecord>)repo.EntityCollections[typeof(BatchRecord)])
            //    yield return item;
            return null;
        }

        protected override void ViewModelLoaded(InMemoryViewModelRepository repo)
        {
            if (repo.EntityCollections.Count == 0)
                return;

            //foreach (var item in (IDictionary<string, BatchRecord>)repo.EntityCollections[typeof(BatchRecord)])
            //{
            //    item.Value.DateOfManufacture = DateTime.SpecifyKind(item.Value.DateOfManufacture, DateTimeKind.Local);
            //    foreach (var denormalizedSubBatchDetails in item.Value.SubBatches)
            //    {
            //        denormalizedSubBatchDetails.ExpiryDate = DateTime.SpecifyKind(denormalizedSubBatchDetails.ExpiryDate, DateTimeKind.Local);
            //    }
            //}
        }
    }
}