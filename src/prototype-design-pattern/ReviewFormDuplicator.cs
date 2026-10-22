namespace PrototypeDesignPattern;

/// <summary>
/// CLIENT — duplicates a form by asking the prototype for a copy. It knows only <see cref="ReviewForm"/>;
/// it never mentions a section, a question or an option, and it never sets an id.
/// </summary>
public static class ReviewFormDuplicator
{
    public static ReviewForm Duplicate(ReviewForm source)
    {
        var copy = source.Clone();
        copy.Title = ManualFormDuplicator.CopyTitle(source.Title);
        return copy;
    }
}
