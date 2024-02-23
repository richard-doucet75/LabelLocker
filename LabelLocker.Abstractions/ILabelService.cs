namespace LabelLocker;

/// <summary>
/// Defines the contract for a service that manages reservations and releases of labels.
/// </summary>
public interface ILabelService
{
    /// <summary>
    /// Asynchronously reserves a label, ensuring it is uniquely held.
    /// This method should return a <see cref="ReservationResult"/>.
    /// </summary>
    /// <param name="label">The name of the label to reserve.</param>
    /// <returns>A task that represents the asynchronous operation, yielding a <see cref="ReservationResult"/>
    /// that indicates whether the reservation was successful and contains a reservation token if so.</returns>
    Task<ReservationResult> ReserveLabelAsync(string label);

    /// <summary>
    /// Asynchronously releases a previously reserved label, making it available for reservation again.
    /// This method should return a <see cref="ReleaseResult"/>.
    /// </summary>
    /// <param name="label">The name of the label to release.</param>
    /// <param name="reservationToken">The reservation token provided at the time of reservation, used for optimistic concurrency control.</param>
    /// <returns>A task that represents the asynchronous operation, yielding a <see cref="ReleaseResult"/>
    /// that indicates whether the release was successful.</returns>
    Task<ReleaseResult> ReleaseLabelAsync(string label, byte[] reservationToken);
}