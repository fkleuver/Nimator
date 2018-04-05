using System.Runtime.CompilerServices;
using Xunit;

namespace Nimator.Tests
{
    /// <inheritdoc />
    /// <summary>
    /// A <see cref="T:Xunit.FactAttribute" /> that causes the simple name of the test method to be used as the DisplayName of the test, rather than the fully qualified name.
    /// </summary>
    public sealed class NamedFactAttribute : FactAttribute
    {
        public NamedFactAttribute([CallerMemberName] string methodName = null)
        {
            DisplayName = methodName;
        }
    }
}
