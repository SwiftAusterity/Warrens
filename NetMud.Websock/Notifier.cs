using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NetMud.Websock
{
  internal class Notifier : IDisposable
  {
    private volatile bool              _enabled;
    private Queue<NotificationMessage> _queue;
    private object                     _sync;
    private ManualResetEvent           _waitHandle;

    public Notifier ()
    {
      _enabled = true;
      _queue = new Queue<NotificationMessage> ();
      _sync = ((ICollection) _queue).SyncRoot;
      _waitHandle = new ManualResetEvent (false);

      ThreadPool.QueueUserWorkItem (
        state => {
          while (_enabled || Count > 0) 
          {
            var msg = dequeue ();

            if (msg != null) 
              Console.WriteLine (msg);
            else 
              Thread.Sleep (500);
          }

          _waitHandle.Set ();
        });
    }

    public int Count {
      get 
      {
        lock (_sync)
          return _queue.Count;
      }
    }

    private NotificationMessage dequeue ()
    {
      lock (_sync)
        return _queue.Count > 0
               ? _queue.Dequeue ()
               : null;
    }

    public void Close ()
    {
      _enabled = false;
      _waitHandle.WaitOne ();
      _waitHandle.Close ();
    }

    public void Notify (NotificationMessage message)
    {
      lock (_sync)
        if (_enabled)
          _queue.Enqueue (message);
    }

    void IDisposable.Dispose ()
    {
      Close ();
    }
  }
}
