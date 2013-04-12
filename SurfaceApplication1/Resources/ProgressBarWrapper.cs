using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace SurfaceApplication1
{
    /// <summary>
    /// Handles the creation of new threads and the display of 
    /// indeterminate progress indicators for long operations. 
    /// @ Mikey Lintz
    /// </summary>
    class ProgressBarWrapper
    {
        private Action _showProgressIndicator;
        private Action _hideProgressIndicator;
        private Dispatcher _uiThreadDispatcher;
        private Thread currentLongOperationThread;

        /// <summary>
        /// The sole constructor.
        /// @ Mikey Lintz
        /// </summary>
        /// <param name="showProgressIndicator">The Action to display the progress indicator</param>
        /// <param name="hideProgressIndicator">The Action to hide the progress indicator</param>
        public ProgressBarWrapper(Action showProgressIndicator, Action hideProgressIndicator)
        {
            this._showProgressIndicator = showProgressIndicator;
            this._hideProgressIndicator = hideProgressIndicator;
        }

        /// <summary>
        /// Shows a progress indicator, runs a long operation in a separate thread, runs the callback
        /// delegate, and hides the progress indicator.
        /// </summary>
        /// <typeparam name="TArgument">The type of argument the longOperation delegate accepts</typeparam>
        /// <typeparam name="TResult">The return type of the longOperation delegate</typeparam>
        /// <param name="longOperation">An operation to be run in a separate thread</param>
        /// <param name="longOperationArgument">The argument to be passed to the long operation. May be null.</param>
        /// <param name="callback">The callback to be run in the main UI thread after the long operation
        /// returns.</param>
        //public void execute<TArgument, TResult>(Func<TArgument, TResult> longOperation, TArgument longOperationArgument, Action<TResult> callback)
        //{
        //    Func<TResult> curriedLongOperation = delegate()
        //    {
        //        return longOperation(longOperationArgument);
        //    };
        //    execute<TResult>(curriedLongOperation, callback);
        //}

        //Modification of the above: Thread constructor accepts both argument and return type and returns thread
        public Thread execute<TArgument, TResult>(Func<TArgument, TResult> longOperation, TArgument longOperationArgument, Action<TResult> callback)
        {
            _showProgressIndicator();
            if (_uiThreadDispatcher == null)
            {
                _uiThreadDispatcher = Dispatcher.CurrentDispatcher;
            }
            Thread currentLongOperationThread = new Thread(
                delegate()
                {
                    TResult result = longOperation(longOperationArgument);
                    _uiThreadDispatcher.BeginInvoke(DispatcherPriority.Background, callback, result);
                    _uiThreadDispatcher.BeginInvoke(_hideProgressIndicator);
                }
            );
            currentLongOperationThread.Start();
            return currentLongOperationThread;
        }

        /// <summary>
        /// Shows a progress indicator, runs a long operation in a separate thread, runs the callback
        /// delegate, and hides the progress indicator.
        /// </summary>
        /// <typeparam name="TResult">The return type of the longOperation delegate</typeparam>
        /// <param name="longOperation">An operation to be run in a separate thread</param>
        /// <param name="callback">The callback to be run in the main UI thread after the long operation
        /// returns.</param>
        public Thread execute<TResult>(Func<TResult> longOperation, Action<TResult> callback)
        {
            _showProgressIndicator();
            if (_uiThreadDispatcher == null)
            {
                _uiThreadDispatcher = Dispatcher.CurrentDispatcher;
            }
            Thread currentLongOperationThread = new Thread(
                delegate()
                {
                    TResult result = longOperation();
                    _uiThreadDispatcher.BeginInvoke(DispatcherPriority.Background, callback, result);
                    _uiThreadDispatcher.BeginInvoke(_hideProgressIndicator);
                }
            );
            currentLongOperationThread.Start();
            return currentLongOperationThread;
        }

        

        public void Show()
        {
            this._showProgressIndicator();
        }

        public void Hide()
        {
            this._hideProgressIndicator();
        }


    }
}
