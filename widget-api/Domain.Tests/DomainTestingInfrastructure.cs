using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CR.AggregateRepository.Core;
using CR.AggregateRepository.Persistance.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Domain.Tests
{
    public static class No
    {
        private static readonly object[] EmptyEvents = new object[0];

        public static object[] Events
        {
            get { return EmptyEvents; }
        }

        private static readonly object[] EmptyCommands = new object[0];

        public static object[] Commands
        {
            get { return EmptyCommands; }
        }
    }

    public static class Require
    {
        public static UsingBuilder That()
        {
            return new UsingBuilder();
        }
    }

    public class UsingBuilder
    {
        public GivenBuilder Using(Func<IAggregateRepository, dynamic> createHandler)
        {
            return new GivenBuilder(createHandler);
        }
    }

    public class GivenBuilder
    {
        private readonly Func<IAggregateRepository, dynamic> _createHandler;

        public GivenBuilder(Func<IAggregateRepository, dynamic> createHandler)
        {
            _createHandler = createHandler;
        }

        public ForIdBuilder Given(params object[] events)
        {
            return new ForIdBuilder(_createHandler, events);
        }
    }

    public class ForIdBuilder
    {
        private readonly Func<IAggregateRepository, dynamic> _createHandler;
        private readonly object[] _events;

        public ForIdBuilder(Func<IAggregateRepository, dynamic> createHandler, object[] events)
        {
            _createHandler = createHandler;
            _events = events;
        }

        public WhenBuilder ForId(object aggregateId)
        {
            return new WhenBuilder(_createHandler, _events, aggregateId);
        }
    }

    public class WhenBuilder
    {
        private readonly Func<IAggregateRepository, dynamic> _createHandler;
        private readonly object[] _events;
        private readonly object _aggregateId;

        public WhenBuilder(Func<IAggregateRepository, dynamic> createHandler, object[] events, object aggregateId)
        {
            _createHandler = createHandler;
            _events = events;
            _aggregateId = aggregateId;
        }

        public ThenBuilder When(params object[] commands)
        {
            return new ThenBuilder(_createHandler, _events, _aggregateId, commands);
        }
    }

    public class ThenBuilder
    {
        private readonly DeepEqualityAsserter _deepEqualityAsserter = new DeepEqualityAsserter();

        private readonly dynamic _handler;
        private readonly object[] _commands;
        private readonly object[] _historicalEvents;
        private readonly object _aggregateId;
        private readonly InMemoryAggregateRepository _repository;

        public ThenBuilder(Func<IAggregateRepository, dynamic> createHandler,
            object[] historicalEvents, object aggregateId,
            object[] commands)
        {
            _historicalEvents = historicalEvents;
            _commands = commands;
            _aggregateId = aggregateId;

            var dict = new Dictionary<object, List<object>>();
            if (_historicalEvents != null && _historicalEvents.Any())
                dict.Add(_aggregateId, _historicalEvents.ToList());
            _repository =
                new InMemoryAggregateRepository(dict);
            _handler = createHandler(_repository);
        }

        public void Produces(params object[] newEvents)
        {
            var actualEvents = PlayTest();
            var expectedEvents = new List<object>();
            expectedEvents.AddRange(_historicalEvents);
            expectedEvents.AddRange(newEvents);
            Assert.That(actualEvents, Has.Length.EqualTo(expectedEvents.Count));
            for (var i = 0; i < actualEvents.Length; i++)
            {
                var actual = actualEvents[i];
                var expected = expectedEvents[i];

                _deepEqualityAsserter.AssertEqual(actual, expected, (a, e) => Assert.That(a, Is.EqualTo(e)));
            }
        }

        public void Throws<TException>() where TException : Exception
        {
            Assert.Throws<TException>(() => PlayTest());
        }

        private object[] PlayTest()
        {
            foreach (var command in _commands)
                _handler.Handle((dynamic) command);

            return _repository.EventStore[_aggregateId].ToArray();
        }
    }

    public class DeepEqualityAsserter
    {
        public void AssertEqual(object graph1, object graph2, Action<object, object> asserter)
        {
            var serialized1 = JsonConvert.SerializeObject(graph1);
            var serialized2 = JsonConvert.SerializeObject(graph2);

            asserter(serialized1, serialized2);
        }
    }
}