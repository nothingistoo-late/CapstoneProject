namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Interface for scheduling exam auto-submit background jobs
/// </summary>
public interface IExamAutoSubmitService
{
    /// <summary>
    /// Schedule an auto-submit job for a learner attempt
    /// </summary>
    /// <param name="attemptId">The learner attempt ID</param>
    /// <param name="deadline">When the job should execute</param>
    /// <returns>Job ID for tracking</returns>
    string ScheduleAutoSubmit(Guid attemptId, DateTime deadline);

    /// <summary>
    /// Cancel a scheduled auto-submit job
    /// </summary>
    /// <param name="jobId">The Hangfire job ID</param>
    void CancelAutoSubmit(string jobId);
}
