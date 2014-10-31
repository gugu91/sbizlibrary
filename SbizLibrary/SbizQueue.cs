using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbiz.Library
{
    public class SbizQueue<T>
    {
        private static Queue<T> _queue;

        public SbizQueue()
        {
            _queue = new Queue<T>();
        }
        /// <summary>
        /// Pay attention each time this method is called returns a element. You should store locally
        /// the element each time you dequeue one
        /// </summary>
        /// <returns>null if queue empty</returns>
        public bool Dequeue(ref T buffer)
        {
            bool retvalue = false;
            lock (_queue)
            {
                if (_queue.Count != 0)
                {
                    retvalue = true;
                    buffer = _queue.Dequeue();
                }
            }

            return retvalue;
        }

        public void Enqueue(T m)
        {
            lock (_queue)
            {
                _queue.Enqueue(m);
            }
        }
    }
}
