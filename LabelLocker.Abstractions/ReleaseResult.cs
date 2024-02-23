namespace LabelLocker;

/// <summary>
/// Represents the result of attempting to release a label.
/// </summary>
public class ReleaseResult
{
    /// <summary>
    /// Gets a value indicating whether the release was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the error message if the release failed.
    /// </summary>
    public string? ErrorMessage { get; }

    private ReleaseResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a success result for the release operation.
    /// </summary>
    /// <returns>A successful release result.</returns>
    public static ReleaseResult SuccessResult() =>
        new ReleaseResult(true, null);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed release result.</returns>
    public static ReleaseResult FailureResult(string errorMessage) =>
        new ReleaseResult(false, errorMessage);
}