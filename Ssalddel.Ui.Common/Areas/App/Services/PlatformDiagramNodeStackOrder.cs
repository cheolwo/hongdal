namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed class PlatformDiagramNodeStackOrder
{
    private readonly List<string> nodeTitles = [];

    public IReadOnlyList<string> NodeTitles => nodeTitles;

    public int Count => nodeTitles.Count;

    public void Synchronize(IEnumerable<string> titles)
    {
        var currentTitles = titles
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var currentTitleSet = currentTitles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        nodeTitles.RemoveAll(title => !currentTitleSet.Contains(title));
        foreach (var title in currentTitles.Where(title =>
                     !nodeTitles.Contains(title, StringComparer.OrdinalIgnoreCase)))
        {
            nodeTitles.Add(title);
        }
    }

    public int GetLayerIndex(string title)
        => nodeTitles.FindIndex(candidate =>
            string.Equals(candidate, title, StringComparison.OrdinalIgnoreCase));

    public bool CanMoveToFront(string? title)
    {
        var index = FindIndex(title);
        return index >= 0 && index < nodeTitles.Count - 1;
    }

    public bool CanMoveToBack(string? title)
    {
        var index = FindIndex(title);
        return index > 0;
    }

    public bool MoveToFront(string? title)
        => Move(title, toFront: true);

    public bool MoveToBack(string? title)
        => Move(title, toFront: false);

    public void Clear()
        => nodeTitles.Clear();

    private bool Move(string? title, bool toFront)
    {
        var index = FindIndex(title);
        if (index < 0 ||
            toFront && index == nodeTitles.Count - 1 ||
            !toFront && index == 0)
        {
            return false;
        }

        var nodeTitle = nodeTitles[index];
        nodeTitles.RemoveAt(index);
        if (toFront)
        {
            nodeTitles.Add(nodeTitle);
        }
        else
        {
            nodeTitles.Insert(0, nodeTitle);
        }

        return true;
    }

    private int FindIndex(string? title)
        => string.IsNullOrWhiteSpace(title)
            ? -1
            : GetLayerIndex(title);
}
