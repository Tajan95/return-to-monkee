public abstract class AbstractRule
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Minimale Gültigkeits-Invariante: jede Regel braucht einen Titel.
    /// Konkrete Regeltypen erweitern dies um eigene Felder (siehe <see cref="TimeLimitRule"/>).
    /// Ersetzt nicht die geplante "Rule Engine" (PRD-Deep-Module, noch nicht implementiert) —
    /// das ist reine Feld-Validierung, keine Aktivierungs- oder Limit-Auswertung.
    /// </summary>
    public virtual bool IsValid() => !string.IsNullOrWhiteSpace(Title);
}