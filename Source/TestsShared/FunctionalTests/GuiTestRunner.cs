using System;
using System.Threading;
using System.Windows.Threading;

namespace TestsShared.FunctionalTests
{
    /// <summary>
    ///     Provides easy way to run in-process gui functional tests for any window
    /// </summary>
    /// <typeparam name="T">
    ///     Type of window to be run. Must have public parameterless constructor.
    /// </typeparam>
    public class GuiTestRunner<T> where T : System.Windows.Window, new()
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        ///     Gets window under test
        /// </summary>
        public T Window { get; private set; }

        /// <summary>
        ///     Gets application under test
        /// </summary>
        public System.Windows.Application Application { get; private set; }

        /// <summary>
        ///     Gets dispatcher used by application
        /// </summary>
        public Dispatcher Dispatcher { get; private set; }


        /// <summary>
        ///     Runs fake application in new thread and gets basic
        ///     automation wrappers.
        /// </summary>
        public void Run()
        {

        }


        /// <summary>
        ///     Tries to stop application - closes the window and waits for confirmation.
        /// </summary>
        public void Stop()
        {
            
        }


        /// <summary>
        ///     Invokes <paramref name="action"/> on application under test dispatcher thread,
        ///     with <see cref="DispatcherPriority.Normal">normal priority</see>.
        /// </summary>
        /// <remarks>
        ///     This method is synchronous. Once the caller regains control, 
        ///     the action has already been executed.
        /// </remarks>
        /// <param name="action">Action to be executed</param>
        /// <exception cref="ArgumentNullException">When <paramref name="action"/> is null</exception>
        public void Invoke(Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }


        /// <summary>
        ///     Waits until input processing and data binding is done.
        /// </summary>
        public void Wait()
        {
            Dispatcher.Invoke(DispatcherPriority.Input,
                              WaitTimeout,
                              new ThreadStart(() => { }));
        }
    }
}