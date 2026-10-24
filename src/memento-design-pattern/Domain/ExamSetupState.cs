namespace MementoDesignPattern.Domain;

public sealed record ExamSetupState(
    ExamConfiguration Configuration,
    IReadOnlyList<ExamSection> Sections)
{
    public int QuestionCount => Sections.Sum(section => section.QuestionCount);

    public string Summary =>
        $"pass={Configuration.PassMarkPercentage}%  "
        + $"duration={Configuration.DurationMinutes}m  "
        + $"attempts={Configuration.AttemptsAllowed}  "
        + $"sections=[{string.Join(", ", Sections.Select(section => $"{section.Title}({section.QuestionCount})"))}]  "
        + $"questions={QuestionCount}";
}
