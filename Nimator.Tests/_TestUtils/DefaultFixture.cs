using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Kernel;

namespace Nimator.Tests
{
    /// <inheritdoc />
    /// <summary>
    /// Fixture configuration for AutoFixture which contains customizations such as factories for otherwise unresolvable types, and automatically mocks interfaces.
    /// </summary>
    /// <remarks>
    /// See also:
    /// - https://github.com/AutoFixture/AutoFixture/wiki/Cheat-Sheet
    /// </remarks>
    public class DefaultFixture : Fixture
    {
        public DefaultFixture()
        {
            Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

            foreach (var builder in GetBuilders())
            {
                Customizations.Add(builder);
            }
        }

        protected virtual IEnumerable<ISpecimenBuilder> GetBuilders()
        {
            yield return new DataCollectorBuilder();
            yield return new DataCollectionResultsBuilder();
        }
    }
}
