using System.ComponentModel;

namespace ZiziBot.Types.Types;

public sealed class ConsoleMenuModel(Func<List<ConsoleMenuModel>> collection) : INotifyPropertyChanged
{
    private bool _expanded;

    public string? Text { get; init; }
    public string? Icon { get; init; }
    public string? Path { get; init; }
    public string? Roles { get; set; }

    public IEnumerable<ConsoleMenuModel>? Items { get; init; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value) return;

            collection().Where(i => i != this).ToList().ForEach(s => s.Expanded = false);

            _expanded = value;
            OnPropertyChanged(nameof(Expanded));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}