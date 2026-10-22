namespace PrototypeDesignPattern;

/// <summary>
/// CONCRETE PROTOTYPE — the aggregate root. <see cref="Clone"/> copies the whole graph by asking
/// every child to clone itself, so the caller never names a child type.
/// </summary>
public sealed class ReviewForm : IPrototype<ReviewForm>
{
    private readonly List<ReviewSection> _sections = [];

    public ReviewForm(int id, string title, int reviewCycleId)
    {
        Id = id;
        Title = title;
        ReviewCycleId = reviewCycleId;
        IsPublished = false;
        IsClosed = false;
    }

    public int Id { get; private set; }

    public string Title { get; set; }

    public int ReviewCycleId { get; }

    public bool IsPublished { get; set; }

    public bool IsClosed { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public Guid? ModifiedBy { get; set; }

    public IReadOnlyList<ReviewSection> Sections => _sections;

    public void AddSection(ReviewSection section) => _sections.Add(section);

    public ReviewForm Clone()
    {
        var copy = new ReviewForm(0, Title, ReviewCycleId)
        {
            IsPublished = false,
            IsClosed = false,
            ModifiedOn = null,
            ModifiedBy = null
        };

        foreach (var section in _sections)
            copy._sections.Add(section.Clone());

        return copy;
    }
}
