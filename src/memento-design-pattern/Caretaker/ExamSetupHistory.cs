using MementoDesignPattern.Memento;
using MementoDesignPattern.Originator;

namespace MementoDesignPattern.Caretaker;

public sealed class ExamSetupHistory
{
    private readonly Stack<ExamSetupMemento> _redo = [];
    private readonly Stack<ExamSetupMemento> _undo = [];

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public int UndoCount => _undo.Count;

    public int RedoCount => _redo.Count;

    public void Save(ExamSetupDraft draft, string label)
    {
        ArgumentNullException.ThrowIfNull(draft);

        _undo.Push(draft.CreateMemento(label));
        _redo.Clear();
    }

    public ExamSetupMemento? Undo(ExamSetupDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (!_undo.TryPop(out var memento))
            return null;

        _redo.Push(draft.CreateMemento());
        draft.Restore(memento);
        return memento;
    }

    public ExamSetupMemento? Redo(ExamSetupDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (!_redo.TryPop(out var memento))
            return null;

        _undo.Push(draft.CreateMemento());
        draft.Restore(memento);
        return memento;
    }
}
