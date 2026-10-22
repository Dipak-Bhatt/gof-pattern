namespace PrototypeDesignPattern;

/// <summary>
/// One question inside a section. It owns the answer options and clones them with itself.
/// </summary>
public sealed class ReviewQuestion : IPrototype<ReviewQuestion>
{
    private readonly List<AnswerOption> _options = [];

    public ReviewQuestion(
        int id,
        int reviewFormId,
        int reviewSectionId,
        string prompt,
        double? weight)
    {
        Id = id;
        ReviewFormId = reviewFormId;
        ReviewSectionId = reviewSectionId;
        Prompt = prompt;
        Weight = weight;
    }

    public int Id { get; private set; }

    public int ReviewFormId { get; private set; }

    public int ReviewSectionId { get; private set; }

    public string Prompt { get; set; }

    public double? Weight { get; }

    public IReadOnlyList<AnswerOption> Options => _options;

    public void AddOption(AnswerOption option) => _options.Add(option);

    public ReviewQuestion Clone()
    {
        var copy = new ReviewQuestion(0, 0, 0, Prompt, Weight);
        foreach (var option in _options)
            copy._options.Add(option.Clone());

        return copy;
    }
}
