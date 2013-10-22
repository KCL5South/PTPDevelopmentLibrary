using System.Collections;
using System.Linq;
using System;
namespace PTPDevelopmentLibrary.EntityGeneration
{
    /// <summary>
    /// Use this object to keep track of tabs when writing out a code file.
    /// </summary>
    public class TabQueue : IEnumerable
    {
        int _count = 0;

        /// <summary>
        /// Gets the number of tabs currently queued.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }
        /// <summary>
        /// Call this method to push (add) a new tab into the queue.
        /// </summary>
        public void Push()
        {
            _count++;
        }
        /// <summary>
        /// Call this method to pop (remove) a tab out of the queue.
        /// </summary>
        public void Pop()
        {
            _count--;
        }

        /// <summary>
        /// This method prints out all the tabs contained in the queue.
        /// </summary>
        /// <returns>A string representing all the tabs contained in the queue.</returns>
        public override string ToString()
        {
            return new string(Enumerable.Repeat<char>('\t', _count).ToArray());
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return Enumerable.Repeat<char>('\t', _count).GetEnumerator();
        }

        #endregion
    }
}