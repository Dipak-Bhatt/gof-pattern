using MementoDesignPattern.Domain;
using MementoDesignPattern.Memento;

namespace MementoDesignPattern.Originator;

public sealed class ExamSetupDraft
{
    private readonly List<ExamSection> _sections;
    private ExamConfiguration _configuration;

    public ExamSetupDraft(ExamConfiguration configuration, IEnumerable<ExamSection> sections)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(sections);

        _configuration = configuration;
        _sections = sections.Select(CopySection).ToList();
    }

    public ExamSetupState State =>
        new(_configuration, _sections.Select(CopySection).ToArray());

    public void UpdateConfiguration(ExamConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
    }

    public void RenameSection(Guid sectionId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var index = IndexOf(sectionId);
        _sections[index] = _sections[index] with { Title = title };
    }

    public void RemoveQuestion(Guid sectionId, Guid questionId)
    {
        var index = IndexOf(sectionId);
        var section = _sections[index];

        if (!section.QuestionIds.Contains(questionId))
            throw new InvalidOperationException($"Question {questionId} is not in section '{section.Title}'.");

        _sections[index] = section with
        {
            QuestionIds = section.QuestionIds.Where(id => id != questionId).ToArray()
        };
    }

    public void AddSection(ExamSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (_sections.Any(existing => existing.Id == section.Id))
            throw new InvalidOperationException($"Section {section.Id} already exists.");

        _sections.Add(CopySection(section));
    }

    public ExamSetupMemento CreateMemento(string? label = null) =>
        new(State, label, DateTimeOffset.UtcNow);

    public void Restore(ExamSetupMemento memento)
    {
        ArgumentNullException.ThrowIfNull(memento);

        var state = memento.ReadState();
        _configuration = state.Configuration;

        _sections.Clear();
        _sections.AddRange(state.Sections.Select(CopySection));
    }

    private static ExamSection CopySection(ExamSection section) =>
        section with { QuestionIds = section.QuestionIds.ToArray() };

    private int IndexOf(Guid sectionId)
    {
        var index = _sections.FindIndex(section => section.Id == sectionId);

        return index >= 0
            ? index
            : throw new InvalidOperationException($"Section {sectionId} is not in this draft.");
    }
}
