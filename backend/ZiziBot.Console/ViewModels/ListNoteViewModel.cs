namespace ZiziBot.Console.ViewModels;

public class ListNoteViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
}