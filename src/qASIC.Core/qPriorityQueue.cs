using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qASIC
{
    public class qPriorityQueue<TElement, TPriority> : IEnumerable<TElement>, IEnumerable
    {
        public qPriorityQueue() { }
        public qPriorityQueue(qPriorityQueue<TElement, TPriority> other)
        {
            Items = new List<Item>(other.Items);
        }

        protected virtual int ComparePriority(TPriority a, TPriority b) =>
            Comparer<TPriority>.Default.Compare(a, b);

        protected List<Item> Items { get; private set; } = new List<Item>();

        public int Count =>
            Items.Count;

        public void Clear() =>
            Items.Clear();

        public bool Contains(TElement element) =>
            Items.Any(x => EqualityComparer<TElement>.Default.Equals(x.element, element));

        public void Enqueue(TElement element, TPriority priority)
        {
            int i = 0;
            for (; i < Items.Count; i++)
            {
                if (i + 1 < Items.Count &&
                    ComparePriority(Items[i + 1].priority, priority) <= 0)
                    break;
            }

            Items.Insert(i, new Item(element, priority));
        }

        public TElement Dequeue()
        {
            var item = Items.First().element;
            Items.RemoveAt(0);
            return item;
        }

        public bool TryDequeue(out TElement result)
        {
            result = default;
            if (Count <= 0)
                return false;

            result = Dequeue();
            return true;
        }

        public TElement Peek() =>
            Items.First().element;

        public bool TryPeek(out TElement element)
        {
            element = default;
            if (Count <= 0)
                return false;

            element = Peek();
            return true;
        }

        public TPriority PeekPriority() =>
            Items.Count != 0 ?
            Items.First().priority :
            default;

        public bool TryPeekPriority(out TPriority element)
        {
            element = default;
            if (Count <= 0)
                return false;

            element = PeekPriority();
            return true;
        }

        public qPriorityQueue<TElement, TOtherPriority> ToOtherPriority<TOtherPriority>(Func<TPriority, TOtherPriority> toOtherPriority)
        {
            var queue = new qPriorityQueue<TElement, TOtherPriority>();

            queue.Items = Items
                .Select(x => new qPriorityQueue<TElement, TOtherPriority>.Item(x.element, toOtherPriority(x.priority)))
                .ToList();

            return queue;
        }

        public IEnumerator GetEnumerator() =>
            Items
            .Select(x => x.element)
            .Reverse()
            .GetEnumerator();

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() =>
            Items
            .Select(x => x.element)
            .Reverse()
            .GetEnumerator();

        protected class Item
        {
            public Item(TElement element, TPriority priority)
            {
                this.element = element;
                this.priority = priority;
            }

            public TElement element;
            public TPriority priority;
        }
    }
}