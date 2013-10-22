using System.Collections.Specialized;
using System.Collections.Generic;
namespace PTPDevelopmentLibrary.Framework
{
    /// <summary>
    /// Represents the business model collection.
    /// </summary>
    public interface IBaseCollectionModel<T> : IBaseModel, INotifyCollectionChanged, IEnumerable<T>
    { }
}