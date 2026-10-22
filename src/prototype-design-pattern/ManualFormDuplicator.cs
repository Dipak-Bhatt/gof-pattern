namespace PrototypeDesignPattern;

/// <summary>
/// Duplicates a form without the pattern. The client must know every level of the graph, copy
/// every property by hand and remember to reset ids and lifecycle flags at each level.
/// </summary>
/// <remarks>
/// This is the code the pattern replaces. Adding a property or a child collection anywhere in the
/// aggregate means editing this method too.
/// </remarks>
public static class ManualFormDuplicator
{
    public static ReviewForm Duplicate(ReviewForm source)
    {
        var formCopy = new ReviewForm(0, CopyTitle(source.Title), source.ReviewCycleId)
        {
            IsPublished = false,
            IsClosed = false,
            ModifiedOn = null,
            ModifiedBy = null
        };

        foreach (var section in source.Sections)
        {
            var sectionCopy = new ReviewSection(
                0,
                0,
                section.Title,
                section.Order,
                section.Weight);
            formCopy.AddSection(sectionCopy);

            foreach (var question in section.Questions)
            {
                var questionCopy = new ReviewQuestion(
                    0,
                    0,
                    0,
                    question.Prompt,
                    question.Weight);
                sectionCopy.AddQuestion(questionCopy);

                foreach (var option in question.Options)
                {
                    questionCopy.AddOption(new AnswerOption(
                        0,
                        0,
                        0,
                        0,
                        option.Label,
                        option.Score));
                }
            }
        }

        return formCopy;
    }

    public static string CopyTitle(string title)
        => "Copy of: " + title.Replace("Copy of: ", string.Empty);
}
