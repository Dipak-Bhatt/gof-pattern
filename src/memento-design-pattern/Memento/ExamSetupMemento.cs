using MementoDesignPattern.Domain;

namespace MementoDesignPattern.Memento;

public sealed class ExamSetupMemento
{
    private readonly ExamSetupState _state;

    internal ExamSetupMemento(ExamSetupState state, string? label, DateTimeOffset capturedAt)
    {
        _state = state;
        Label = label;
        CapturedAt = capturedAt;
    }

    public string? Label { get; }

    public DateTimeOffset CapturedAt { get; }

    internal ExamSetupState ReadState() => _state;
}
