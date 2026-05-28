#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Contract for the <see cref="Error"/> value type.
/// </summary>
public interface IError<out TSelf> where TSelf : IError<TSelf>
{
    /// <summary>
    /// The error message.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// The originating exception, if any.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Enables <c>Error err = "Meldung";</c>.
    /// </summary>
    static abstract implicit operator TSelf(string error);

    /// <summary>
    /// Enables <c>Error err = exception;</c>.
    /// </summary>
    static abstract implicit operator TSelf(Exception exception);
}
