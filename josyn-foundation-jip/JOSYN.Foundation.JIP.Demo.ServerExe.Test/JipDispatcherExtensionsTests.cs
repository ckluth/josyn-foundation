using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using NUnit.Framework;

namespace JOSYN.Foundation.JIP.Demo.ServerExe.Test;

[TestFixture]
public sealed class JipDispatcherExtensionsTests
{
    /// <summary>
    /// Verifies that RegisterAll covers all methods defined in IJosynApplicationProtocol.
    /// Fails when a method is added to the interface with an unsupported signature,
    /// or when the reflection mapping breaks.
    /// </summary>
    [Test]
    public void RegisterAll_CoversAllMethodsOf_IJosynApplicationProtocol()
    {
        var expectedKeys = typeof(IJosynApplicationProtocol)
            .GetMethods()
            .Select(m => m.Name)
            .ToHashSet();

        var dispatcher = new JipDispatcher().RegisterAll<IJosynApplicationProtocol>(new FakeJosynApplicationProtocol());

        Assert.That(dispatcher.RegisteredKeys, Is.SupersetOf(expectedKeys),
            "RegisterAll hat nicht alle Methoden von IJosynApplicationProtocol registriert. " +
            "Fehlende Keys: " + string.Join(", ", expectedKeys.Except(dispatcher.RegisteredKeys)));
    }

    private sealed class FakeJosynApplicationProtocol : IJosynApplicationProtocol
    {
        public Task<Result<string>> GetRawArguments() => Task.FromResult(Result<string>.Success(""));
        public Task<Result> PutRawResult(string result) => Task.FromResult(Result.Success);
    }
}
