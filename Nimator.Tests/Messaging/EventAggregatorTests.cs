using System;
using System.Collections.Generic;
using FluentAssertions;
using Nimator.Messaging;

namespace Nimator.Tests.Messaging
{
    public class EventAggregatorTests
    {
        private static EventAggregator Sut => EventAggregator.Instance;
        
        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            typeof(EventAggregator).VerifyInstanceMethodGuards(Sut).Should().Be(1);
        }

        [NamedFact]
        public void StaticMethods_ShouldHaveCorrectGuardClauses()
        {
            typeof(EventAggregator).VerifyStaticMethodGuards().Should().Be(0);
        }

        [NamedFact]
        public void Publish_ShouldNotThrow_WhenThereAreNoSubscriptions()
        {
            Sut.Publish("");
        }

        [NamedFact]
        public void Publish_ShouldNotThrow_WhenThereAreNoHandlers_AndEventIsNull()
        {
            Sut.Publish((object)null);
        }
        
        [NamedFact]
        public void Unsubscribe_ShouldNotThrow_WhenThereAreNoSubscriptions()
        {
            Sut.Unsubscribe(Guid.NewGuid());
        }
        
        [NamedFact]
        public void Publish_ShouldInvokeSubscriber_WhenTypesMatch()
        {
            var message = "";
            void Subscriber(string msg) => message = msg;

            Sut.ClearSubscriptions();
            Sut.Subscribe((Action<string>) Subscriber);

            Sut.Publish("Foo");

            message.Should().Be("Foo");
        }
        
        [NamedFact]
        public void Publish_ShouldNotInvokeSubscriber_WhenTypesDoNotMatch()
        {
            var message = "";
            void Subscriber(string msg) => message = msg;

            Sut.ClearSubscriptions();
            Sut.Subscribe((Action<string>) Subscriber);

            Sut.Publish((object)"Foo");

            message.Should().Be("");
        }
        
        [NamedFact]
        public void Publish_ShouldInvokeSubscribers_IfAnotherSubscriberThrows()
        {
            var exception = new Exception();
            var messages = new List<string>();
            void BadSubscriber(string msg) => throw exception;
            void GoodSubscriber(string msg) => messages.Add("Foo" + msg);

            Sut.ClearSubscriptions();
            
            Sut.Subscribe((Action<string>) BadSubscriber);
            Sut.Subscribe((Action<string>) GoodSubscriber);

            Sut.Publish("Bar");
            Sut.Publish("Baz");
            
            messages.Count.Should().Be(2);
            messages[0].Should().Be("FooBar");
            messages[1].Should().Be("FooBaz");
        }
        
        
        [NamedFact]
        public void IsSubscribed_ShouldReturnCorrectResult()
        {
            void SubscriberOne(string msg){}
            void SubscriberTwo(string msg){}

            Sut.ClearSubscriptions();
            
            var tokenOne = Sut.Subscribe((Action<string>) SubscriberOne);
            var tokenTwo = Sut.Subscribe((Action<string>) SubscriberTwo);

            Sut.IsSubscribed(tokenOne).Should().BeTrue();
            Sut.IsSubscribed(tokenTwo).Should().BeTrue();

            Sut.Unsubscribe(tokenOne);
            Sut.IsSubscribed(tokenOne).Should().BeFalse();
            Sut.IsSubscribed(tokenTwo).Should().BeTrue();

            Sut.ClearSubscriptions();

            Sut.IsSubscribed(tokenOne).Should().BeFalse();
            Sut.IsSubscribed(tokenTwo).Should().BeFalse();
        }
    }
}
