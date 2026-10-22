namespace PrototypeDesignPattern;

/// <summary>
/// A section of the form. It clones its questions, and each question clones its own options.
/// </summary>
public sealed class ReviewSection : IPrototype<ReviewSection>
{
    private readonly List<ReviewQuestion> _questions = [];

    public ReviewSection(
        int id,
        int reviewFormId,
        string title,
        int order,
        double? weight)
    {
        Id = id;
        ReviewFormId = reviewFormId;
        Title = title;
        Order = order;
        Weight = weight;
    }

    public int Id { get; private set; }

    public int ReviewFormId { get; private set; }

    public string Title { get; set; }

    public int Order { get; }

    public double? Weight { get; }

    public IReadOnlyList<ReviewQuestion> Questions => _questions;

    public void AddQuestion(ReviewQuestion question) => _questions.Add(question);

    public ReviewSection Clone()
    {
        var copy = new ReviewSection(0, 0, Title, Order, Weight);
        foreach (var question in _questions)
            copy._questions.Add(question.Clone());

        return copy;
    }
}
