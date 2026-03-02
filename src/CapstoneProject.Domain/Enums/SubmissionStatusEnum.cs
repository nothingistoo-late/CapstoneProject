namespace CapstoneProject.Domain.Enums;

/// <summary>
/// Trạng thái bài nộp (solution) của người chơi.
/// </summary>
public enum SubmissionStatusEnum
{
    Pending = 0,
    Running = 1,
    Accepted = 2,
    WrongAnswer = 3,
    TimeLimitExceeded = 4,
    ConstraintViolation = 5,
    InternalError = 6
}
