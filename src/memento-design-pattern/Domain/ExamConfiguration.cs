namespace MementoDesignPattern.Domain;

public sealed record ExamConfiguration(
    short PassMarkPercentage,
    short DurationMinutes,
    short AttemptsAllowed);
