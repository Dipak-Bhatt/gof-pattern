namespace PrototypeDesignPattern;

/// <summary>
/// The leaf of the aggregate: one answer the reviewer can choose.
/// </summary>
public sealed class AnswerOption : IPrototype<AnswerOption>
{
    public AnswerOption(
        int id,
        int reviewFormId,
        int reviewSectionId,
        int reviewQuestionId,
        string label,
        double? score)
    {
        Id = id;
        ReviewFormId = reviewFormId;
        ReviewSectionId = reviewSectionId;
        ReviewQuestionId = reviewQuestionId;
        Label = label;
        Score = score;
    }

    public int Id { get; private set; }

    public int ReviewFormId { get; private set; }

    public int ReviewSectionId { get; private set; }

    public int ReviewQuestionId { get; private set; }

    public string Label { get; set; }

    public double? Score { get; }

    public AnswerOption Clone()
        => new(0, 0, 0, 0, Label, Score);
}
