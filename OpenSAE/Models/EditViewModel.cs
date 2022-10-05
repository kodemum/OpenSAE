using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenSAE.Models
{
    /// <summary>
    /// Base class for view models that represent a separate edit view
    /// </summary>
    public abstract class EditViewModel : ObservableObject
    {
        public abstract string Title { get; }

        public virtual string Subtitle => string.Empty;

        public abstract void ApplyChanges();

        public abstract void Cancel();
    }
}
