namespace PTPDevelopmentLibrary.Framework
{
    /// <summary>
    /// Represents the most basic business model.
    /// </summary>
    public interface IBaseModel : System.ComponentModel.INotifyPropertyChanged
#if !SILVERLIGHT
        , System.ComponentModel.INotifyPropertyChanging
#endif
    { }
}