namespace MementoDesignPattern.Domain;

public sealed record ExamSection(
    Guid Id,
    string Title,
    IReadOnlyList<Guid> QuestionIds)
{
    public int QuestionCount => QuestionIds.Count;
}
