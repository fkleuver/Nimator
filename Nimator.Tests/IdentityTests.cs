using System;
using FluentAssertions;
using Nimator.Util;

namespace Nimator.Tests
{
    public class IdentityTests
    {
        [NamedFact]
        public void Constructor_ShouldThrow_WhenNameIsNull()
        {
            Action act = () => new Identity((string)null);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedFact]
        public void Constructor_ShouldThrow_WhenNameIsEmpty()
        {
            Action act = () => new Identity("");

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedFact]
        public void Constructor_ShouldThrow_WhenNameIsWhiteSpace()
        {
            Action act = () => new Identity(" ");

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedFact]
        public void Constructor_ShouldThrow_WhenTypeIsNull()
        {
            Action act = () => new Identity((Type)null);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldInitializeWithProvidedName(string name)
        {
            var sut = new Identity(name);

            sut.Name.Should().BeEquivalentTo(name);
        }

        [NamedFact]
        public void Constructor_ShouldInitializeWithTypeName()
        {
            var sut = new Identity(typeof(DummyClass));

            sut.Name.Should().BeEquivalentTo(@"DummyClass");
        }

        [NamedFact]
        public void Constructor_ShouldInitializeWithTypeName_WhenTypeHasGenericArguments()
        {
            var sut = new Identity(typeof(DummyClassWithGenerics<DummyClass>));

            sut.Name.Should().BeEquivalentTo(@"DummyClassWithGenerics<DummyClass>");
        }

        [NamedFact]
        public void Constructor_ShouldInitializeWithTypeName_WhenTypeHasNestedGenericArguments()
        {
            var sut = new Identity(typeof(DummyClassWithGenerics<DummyClassWithGenerics<DummyClass>>));

            sut.Name.Should().BeEquivalentTo(@"DummyClassWithGenerics<DummyClassWithGenerics<DummyClass>>");
        }
        
        [NamedTheory, DefaultFixture]
        public void Equals_ShouldReturnTrue_WhenNamesAreEqual(string name)
        {
            var sut1 = new Identity(name);
            var sut2 = new Identity(name);
            
            // ReSharper disable | Testing the full IEquatable here, so leave this explicit
            (sut1.Equals(sut2)).Should().BeTrue();
            (sut1 == sut2).Should().BeTrue();
            (sut1 != sut2).Should().BeFalse();
            (object.Equals(sut1, sut2)).Should().BeTrue();
            // ReSharper enable
        }
        
        [NamedTheory, DefaultFixture]
        public void Equals_ShouldReturnFalse_WhenNamesAreDifferent(string name1, string name2)
        {
            var sut1 = new Identity(name1);
            var sut2 = new Identity(name2);
            
            // ReSharper disable | Testing the full IEquatable here, so leave this explicit
            (sut1.Equals(sut2)).Should().BeFalse();
            (sut1 == sut2).Should().BeFalse();
            (sut1 != sut2).Should().BeTrue();
            (object.Equals(sut1, sut2)).Should().BeFalse();
            // ReSharper enable
        }
    }
    
    internal sealed class DummyClass { }
    internal sealed class DummyClassWithGenerics<T> { }
}
