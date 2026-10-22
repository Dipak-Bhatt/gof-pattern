using System.Reflection;
using PrototypeDesignPattern;

Console.WriteLine("=== Prototype Design Pattern — Duplicating a Review Form ===\n");
Console.WriteLine("PROTOTYPE          IPrototype<T>.Clone()");
Console.WriteLine("CONCRETE PROTOTYPE ReviewForm, ReviewSection, ReviewQuestion, AnswerOption");
Console.WriteLine("CLIENT             ReviewFormDuplicator.Duplicate(form)\n");

const int formId = 17;
const int reviewCycleId = 3;

ReviewForm BuildOriginalForm()
{
    var form = new ReviewForm(formId, "Annual Review 2026", reviewCycleId)
    {
        IsPublished = true
    };

    var goals = new ReviewSection(101, formId, "Goals", order: 1, weight: 60);
    var revenue = new ReviewQuestion(501, formId, 101, "Achieved the revenue target", weight: 100);
    revenue.AddOption(new AnswerOption(9001, formId, 101, 501, "Met", 100));
    revenue.AddOption(new AnswerOption(9002, formId, 101, 501, "Exceeded", 120));
    goals.AddQuestion(revenue);

    var behaviour = new ReviewSection(102, formId, "Behaviour", order: 2, weight: 40);
    var teamwork = new ReviewQuestion(502, formId, 102, "Works well with the team", weight: 100);
    teamwork.AddOption(new AnswerOption(9003, formId, 102, 502, "Yes", 100));
    teamwork.AddOption(new AnswerOption(9004, formId, 102, 502, "No", 0));
    behaviour.AddQuestion(teamwork);

    form.AddSection(goals);
    form.AddSection(behaviour);
    return form;
}

static string Describe(ReviewForm form)
{
    var questionCount = form.Sections.Sum(section => section.Questions.Count);
    var optionCount = form.Sections.Sum(section => section.Questions.Sum(question => question.Options.Count));
    return $"{form.Title} | sections={form.Sections.Count}, questions={questionCount}, options={optionCount}, " +
           $"formId={form.Id}, published={form.IsPublished}";
}

var original = BuildOriginalForm();

Console.WriteLine("--- Without the pattern: the caller copies every level by hand ---");
var manualCopy = ManualFormDuplicator.Duplicate(original);
Console.WriteLine($"  {Describe(manualCopy)}");
Console.WriteLine("  ManualFormDuplicator knows all four types and resets every id and flag.\n");

Console.WriteLine("--- With the prototype: one call copies the whole graph ---");
var prototypeCopy = ReviewFormDuplicator.Duplicate(original);
Console.WriteLine($"  {Describe(prototypeCopy)}");
Console.WriteLine("  ReviewFormDuplicator calls source.Clone(); every child clones itself.\n");

Console.WriteLine("--- Why the copy must be deep: a shallow copy shares the children ---");
static ReviewForm ShallowCopy(ReviewForm source)
{
    var method = typeof(object).GetMethod(nameof(MemberwiseClone), BindingFlags.Instance | BindingFlags.NonPublic)!;
    return (ReviewForm)method.Invoke(source, null)!;
}

var shallowCopy = ShallowCopy(original);
shallowCopy.Sections[0].Title = "MUTATED (shallow copy)";
Console.WriteLine($"  shallow copy section: '{shallowCopy.Sections[0].Title}'");
Console.WriteLine($"  original section:     '{original.Sections[0].Title}'   <-- same object\n");
original.Sections[0].Title = "Goals";

Console.WriteLine("--- Clone() returns an independent graph ---");
var deepCopy = original.Clone();
deepCopy.Title = ManualFormDuplicator.CopyTitle(original.Title);
deepCopy.Sections[0].Title = "Goals (copy)";
deepCopy.Sections[0].Questions[0].Options[0].Label = "Met (copy)";
Console.WriteLine($"  copy:     title='{deepCopy.Title}', section='{deepCopy.Sections[0].Title}', " +
                  $"option='{deepCopy.Sections[0].Questions[0].Options[0].Label}'");
Console.WriteLine($"  original: title='{original.Title}', section='{original.Sections[0].Title}', " +
                  $"option='{original.Sections[0].Questions[0].Options[0].Label}'");
Console.WriteLine($"  same section object? {ReferenceEquals(original.Sections[0], deepCopy.Sections[0])}");
Console.WriteLine($"  same option object?  {ReferenceEquals(original.Sections[0].Questions[0].Options[0], deepCopy.Sections[0].Questions[0].Options[0])}\n");

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("Prototype moves duplication into the object that owns the data: each level knows how to");
Console.WriteLine("copy itself, so the client asks once and cannot miss a child collection. New fields stay");
Console.WriteLine("in the class that declares them instead of in a growing copy loop in the client.");
Console.WriteLine("Use it when creating an object from scratch is costly or when the graph is deep and");
Console.WriteLine("its concrete types should stay hidden from the caller.");
