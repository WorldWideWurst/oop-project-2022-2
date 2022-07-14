using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Erstellt von Richard Förster

namespace Project.UI.MVVM
{
    /// <summary>
    /// https://stackoverflow.com/a/2853695/17248078
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    public class BoundObservableCollection<T, TSource> : ObservableCollection<T>
    {
        private readonly ObservableCollection<TSource> _source;
        private readonly Func<TSource, T> _converter;
        private readonly Func<T, TSource, bool> _isSameSource;

        public BoundObservableCollection(
            ObservableCollection<TSource> source,
            Func<TSource, T> converter,
            Func<T, TSource, bool> isSameSource)
            : base()
        {
            _source = source;
            _converter = converter;
            _isSameSource = isSameSource;

            // Copy items
            AddItems(_source);

            // Subscribe to the source's CollectionChanged event
            _source.CollectionChanged += new NotifyCollectionChangedEventHandler(_source_CollectionChanged);
        }

        private void AddItems(IEnumerable<TSource> items)
        {
            foreach (var sourceItem in items)
            {
                Add(_converter(sourceItem));
            }
        }

        void _source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems.Cast<TSource>());
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Not sure what to do here...
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var sourceItem in e.OldItems.Cast<TSource>())
                    {
                        var toRemove = this.First(item => _isSameSource(item, sourceItem));
                        this.Remove(toRemove);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = e.NewStartingIndex; i < e.NewItems.Count; i++)
                    {
                        this[i] = _converter((TSource)e.NewItems[i]);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Clear();
                    this.AddItems(_source);
                    break;
                default:
                    break;
            }
        }
    }
}
