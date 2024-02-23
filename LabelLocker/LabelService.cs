using LabelLocker.Repositories;
using LabelLocker.Repositories.Entities;

namespace LabelLocker
{
    /// <summary>
    /// Provides services for managing the reservation and release of labels,
    /// ensuring that labels are uniquely held when reserved and properly released when no longer needed.
    /// This service also handles concurrency conflicts to maintain data integrity.
    /// </summary>
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelService"/> class with a specific label repository.
        /// </summary>
        /// <param name="labelRepository">The repository used for performing operations on labels.</param>
        public LabelService(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }
        
        /// <summary>
        /// Asynchronously attempts to reserve a label, ensuring it is uniquely held.
        /// </summary>
        /// <param name="label">The name of the label to reserve. The name is case-insensitive and cannot be null or whitespace.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the outcome of the reservation attempt,
        /// including a reservation token if successful or an error message if the operation failed due to concurrency conflicts or if the label is already reserved.
        /// </returns>
        /// <remarks>
        /// This method checks if the label is already reserved and returns a failure result if so. It also handles concurrency conflicts
        /// by checking the provided reservation token against the current token stored in the database.
        /// </remarks>
        public async Task<ReservationResult> ReserveLabelAsync(string label)
        {
            var lowerCaseLabel = label.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(lowerCaseLabel))
            {
                return ReservationResult.FailureResult("Label name cannot be null or whitespace.");
            }

            var labelEntity = await _labelRepository.FindLabelAsync(lowerCaseLabel);
            if (labelEntity == null)
            {
                labelEntity = new LabelEntity { Name = lowerCaseLabel, State = LabelState.Reserved };
                var reservationToken = await _labelRepository.AddOrUpdateLabelAsync(labelEntity);
                return reservationToken == null
                    ? ReservationResult.FailureResult("Concurrency conflict detected during reservation.")
                    : ReservationResult.SuccessResult(reservationToken);
            }

            if (labelEntity.State == LabelState.Reserved)
            {
                return ReservationResult.FailureResult("Label is already reserved.");
            }

            labelEntity.State = LabelState.Reserved;
            var updateToken = await _labelRepository.AddOrUpdateLabelAsync(labelEntity);
            return updateToken == null
                ? ReservationResult.FailureResult("Concurrency conflict detected during update.")
                : ReservationResult.SuccessResult(updateToken);
        }
        
        /// <summary>
        /// Asynchronously attempts to release a previously reserved label, making it available again for reservation.
        /// </summary>
        /// <param name="label">The name of the label to release. The name is case-insensitive.</param>
        /// <param name="reservationToken">The reservation token obtained during the label's reservation,
        /// used for optimistic concurrency control.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result indicates whether the release was successful,
        /// including handling of concurrency conflicts.
        /// </returns>
        /// <remarks>
        /// This method verifies the reservation token matches the current token stored in the database to prevent unauthorized releases.
        /// It returns a failure result if there's a concurrency conflict or if the label is not currently reserved.
        /// </remarks>
        public async Task<ReleaseResult> ReleaseLabelAsync(string label, byte[] reservationToken)
        {
            var lowerCaseLabel = label.ToLowerInvariant();
            var labelEntity = await _labelRepository.FindLabelAsync(lowerCaseLabel);
            if (labelEntity == null)
            {
                // Considered a success because the label is effectively available.
                return ReleaseResult.SuccessResult(); 
            }

            if (!labelEntity.ReservationToken.SequenceEqual(reservationToken))
            {
                return ReleaseResult.FailureResult("Concurrency conflict detected.");
            }

            if (labelEntity.State != LabelState.Reserved)
            {
                // If the label is not reserved, no action is required, and the operation is considered successful.
                return ReleaseResult.SuccessResult();
            }
            
            labelEntity.State = LabelState.Available;
            var success = await _labelRepository.AddOrUpdateLabelAsync(labelEntity);
            return success == null
                ? ReleaseResult.FailureResult("Failed to update the label due to a concurrency conflict.")
                : ReleaseResult.SuccessResult();
        }
    }
}