using System;
using CommandHandlers;
using Commands;
using CR.AggregateRepository.Core.Exceptions;
using Events;
using NUnit.Framework;
using TimeKeeping;

namespace Domain.Tests
{
    [TestFixture]
    class when_creating_a_widget
    {
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Clock.Initialize(() => new DateTime(2015, 12, 10));
        }

        [Test]
        public void with_valid_command_should_produce_widget_created_event()
        {
            var id = Guid.NewGuid();
            var name = "WOFTAM";

            Require.That()
                .Using(repo => new WidgetCommandHandler(repo))
                .Given(No.Events)
                .ForId(new WidgetId(id))
                .When(new CreateWidget(id, name))
                .Produces(new WidgetCreated(id, name, Clock.Now));
        }

        [Test]
        public void that_already_exists_should_throw_aggregate_version_exception()
        {
            var id = Guid.NewGuid();
            var name = "WOFTAM";

            Require.That()
                .Using(repo => new WidgetCommandHandler(repo))
                .Given(new WidgetCreated(id, name, Clock.Now))
                .ForId(new WidgetId(id))
                .When(new CreateWidget(id, name))
                .Throws<AggregateVersionException>();
        }
    }
}
