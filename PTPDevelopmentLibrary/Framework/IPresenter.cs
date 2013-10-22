namespace PTPDevelopmentLibrary.Framework
{
    /// <summary>
    /// The contract used to signify that an object is the presenter within the MVP methodology.
    /// Notice: You should not inherit from this interface directly.  You should inherit from the 
    /// generic version of this interface.  This interface is used as a base, so that a reference
    /// can be created to the presenter without knowing the types of the view and model.
    /// </summary>
    public interface IPresenter
    {
        /// <summary>
        /// Gets a reference to the MVP view.
        /// </summary>
        object View { get; set; }
        /// <summary>
        /// Gets a reference to the MVP model.
        /// </summary>
        object Model { get; set; }
    }

    /// <summary>
    /// The contract used to signify that an object is a presenter within the MVP methodology.
    /// </summary>
    /// <typeparam name="TView">The type of the target view.</typeparam>
    /// <typeparam name="TModel">The type of the target model.</typeparam>
#pragma warning disable 0108
    public interface IPresenter<TView, TModel> : IPresenter
    {
        /// <summary>
        /// Gets a reference to the MVP view.
        /// </summary>
        TView View { get; }
        /// <summary>
        /// Gets a reference to the MVP model.
        /// </summary>
        TModel Model { get; }
    }
#pragma warning restore 0108
}