using MementoDesignPattern.Caretaker;
using MementoDesignPattern.Domain;
using MementoDesignPattern.Originator;

Console.WriteLine("=== Memento Design Pattern — Exam Setup Undo ===\n");
Console.WriteLine("ORIGINATOR  ExamSetupDraft    owns the live exam configuration and sections");
Console.WriteLine("MEMENTO     ExamSetupMemento  opaque snapshot that only the originator can read");
Console.WriteLine("CARETAKER   ExamSetupHistory  stores snapshots and manages undo / redo\n");

var knowledgeSectionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var scenarioSectionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
var behaviourSectionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
var question1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
var question2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
var question3 = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
var question4 = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

var draft = new ExamSetupDraft(
    new ExamConfiguration(50, 60, 3),
    [
        new ExamSection(knowledgeSectionId, "Knowledge", [question1, question2]),
        new ExamSection(scenarioSectionId, "Scenario", [question3]),
    ]);
var history = new ExamSetupHistory();

history.Save(draft, "opened");
Show("1. A checkpoint is captured before editing", draft, history);

draft.UpdateConfiguration(new ExamConfiguration(70, 90, 4));
Show("2. The author changes the configuration in place", draft, history);

history.Save(draft, "before structure edit");
draft.RenameSection(scenarioSectionId, "Case study");
draft.RemoveQuestion(knowledgeSectionId, question2);
draft.AddSection(new ExamSection(behaviourSectionId, "Behaviour", [question4]));
Show("3. The author changes the structure in place", draft, history);

ShowUndo("4. Undo restores the saved checkpoint", draft, history);
ShowUndo("5. Undo again returns to the opening state", draft, history);

ShowRedo("6. Redo returns to the configuration edit", draft, history);
ShowRedo("7. Redo returns to the structure edit", draft, history);

ShowUndo("8. Undo reopens the redo branch", draft, history);

history.Save(draft, "before a new edit");
draft.UpdateConfiguration(new ExamConfiguration(80, 75, 5));
Show("9. A new edit after undo clears the redo branch", draft, history);

Console.WriteLine("=== Key Insight ==="); 
Console.WriteLine("Memento moves state capture and restoration into the object that owns the state.");
Console.WriteLine("The caretaker never reads a snapshot; it stores opaque mementos and hands them");
Console.WriteLine("back to the originator. Undo therefore uses one protocol — CreateMemento and");
Console.WriteLine("Restore — instead of the client trying to reverse every field it changed.");

static void Show(string title, ExamSetupDraft draft, ExamSetupHistory history)
{
    Console.WriteLine($"--- {title} ---");
    ShowState(draft, history);
}

static void ShowUndo(string title, ExamSetupDraft draft, ExamSetupHistory history)
{
    var memento = history.Undo(draft);
    Console.WriteLine($"--- {title} -> {memento?.Label ?? "previous state"} ---");
    ShowState(draft, history);
}

static void ShowRedo(string title, ExamSetupDraft draft, ExamSetupHistory history)
{
    history.Redo(draft);
    Console.WriteLine($"--- {title} ---");
    ShowState(draft, history);
}

static void ShowState(ExamSetupDraft draft, ExamSetupHistory history)
{
    Console.WriteLine($"  {draft.State.Summary}");
    Console.WriteLine($"  history: undo={history.UndoCount}  redo={history.RedoCount}\n");
}
