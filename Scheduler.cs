using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace AutoTracker
{
    /// <summary>
    /// Implements a single-threaded scheduler to run deferred code.
    /// </summary>
    class Scheduler
    {
        ISynchronizeInvoke owner;

        const int MaxTimerReserve = 4;

        List<System.Timers.Timer> FreeTimers = new List<System.Timers.Timer>();
        List<System.Timers.Timer> BusyTimers = new List<System.Timers.Timer>();
        
        public Scheduler(ISynchronizeInvoke owner) {
            this.owner = owner;
        }

        public void Queue(TickAction action, double milliseconds) {
            System.Timers.Timer timer;
            if (FreeTimers.Count == 0) {
                timer = new System.Timers.Timer(milliseconds);
                timer.AutoReset = false;
                timer.SynchronizingObject = owner;
                BusyTimers.Add(timer);
            } else {
                int i = FreeTimers.Count - 1;
                timer = FreeTimers[i];
                FreeTimers.RemoveAt(i);

                timer.Interval = milliseconds;
            }

            System.Timers.ElapsedEventHandler handler = delegate(object sender, System.Timers.ElapsedEventArgs e) {
                HandleTimerTick(timer, action);
            };

            timer.Elapsed += handler;
        }

        /// <summary>
        /// Can be called from any thread. Releases the specified timer and invokes the 
        /// specified action on the owning thread.
        /// </summary>
        private void HandleTimerTick(System.Timers.Timer timer, TickAction action) {
            // Marshal to main thread if required
            if (owner.InvokeRequired) {
                owner.Invoke((TickAction)delegate() { HandleTimerTick(timer, action); }, null);
            } else {
                BusyTimers.Remove(timer);
                if (FreeTimers.Count >= MaxTimerReserve) {
                    timer.Dispose();
                } else {
                    FreeTimers.Add(timer);
                }

                action();
            }
        }

    }

    public delegate void TickAction();
}
