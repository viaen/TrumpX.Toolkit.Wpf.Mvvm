using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrumpX.Toolkit.Wpf.Mvvm
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static bool EnableVerifyPropertyName { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = default)
        {
            VerifyPropertyName(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }
            MemberExpression expression = propertyExpression.Body as MemberExpression;
            if (expression == null)
            {
                throw new ArgumentException("Invalid argument.", nameof(propertyExpression));
            }
            PropertyInfo info = expression.Member as PropertyInfo;
            if (info == null)
            {
                throw new ArgumentException("Argument is not a property.", nameof(propertyExpression));
            }
            OnPropertyChanged(info.Name);
        }

        protected void OnPropertyChanged<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (var item in propertyExpressions)
            {
                OnPropertyChanged(item);
            }
        }

        private void VerifyPropertyName(string propertyName)
        {
            if (EnableVerifyPropertyName)
            {
                if (string.IsNullOrEmpty(propertyName) || GetType().GetProperty(propertyName) != null) return;
                throw new ArgumentException("Property not found.", nameof(propertyName));
            }
        }

        protected bool SetProperty<T>(ref T field, T newValue, IEqualityComparer<T> comparer = default, [CallerMemberName] string propertyName = default)
        {
            if ((comparer ?? EqualityComparer<T>.Default).Equals(field, newValue))
            {
                return false;
            }
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(T oldValue, T newValue, Action<T> callback, IEqualityComparer<T> comparer = default, [CallerMemberName] string propertyName = default)
        {
            if ((comparer ?? EqualityComparer<T>.Default).Equals(oldValue, newValue))
            {
                return false;
            }
            callback(newValue);
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<TModel, T>(T oldValue, T newValue, TModel model, Action<TModel, T> callback, IEqualityComparer<T> comparer = default, [CallerMemberName] string propertyName = default)
            where TModel : class
        {
            if ((comparer ?? EqualityComparer<T>.Default).Equals(oldValue, newValue))
            {
                return false;
            }
            callback(model, newValue);
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetPropertyAndNotifyOnCompletion(ref TaskNotifier taskNotifier, Task newValue, Action<Task> callback = default, [CallerMemberName] string propertyName = default)
        {
            if (taskNotifier == null) { taskNotifier = new TaskNotifier(); }
            return SetPropertyAndNotifyOnCompletion(taskNotifier, newValue, callback, propertyName);
        }

        protected bool SetPropertyAndNotifyOnCompletion<T>(ref TaskNotifier<T> taskNotifier, Task<T> newValue, Action<Task<T>> callback = default, [CallerMemberName] string propertyName = default)
        {
            if (taskNotifier == null) { taskNotifier = new TaskNotifier<T>(); }
            return SetPropertyAndNotifyOnCompletion(taskNotifier, newValue, callback, propertyName);
        }

        private bool SetPropertyAndNotifyOnCompletion<TTask>(ITaskNotifier<TTask> taskNotifier, TTask newValue, Action<TTask> callback, [CallerMemberName] string propertyName = default)
            where TTask : Task
        {
            if (ReferenceEquals(taskNotifier.Task, newValue))
            {
                return false;
            }

            var isAlreadyCompletedOrNull = newValue?.IsCompleted ?? true;

            taskNotifier.Task = newValue;

            OnPropertyChanged(propertyName);

            if (isAlreadyCompletedOrNull)
            {
                callback?.Invoke(newValue);

                return true;
            }

            async void MonitorTask()
            {
                try
                {
                    await newValue;
                }
                catch
                {
                    // ignored
                }

                if (ReferenceEquals(taskNotifier.Task, newValue))
                {
                    OnPropertyChanged(propertyName);
                }

                callback?.Invoke(newValue);
            }

            MonitorTask();

            return true;
        }

        private interface ITaskNotifier<TTask>
            where TTask : Task
        {
            TTask Task { get; set; }
        }

        protected sealed class TaskNotifier : ITaskNotifier<Task>
        {
            internal TaskNotifier()
            {
            }

            private Task _task;

            Task ITaskNotifier<Task>.Task
            {
                get => _task;
                set => _task = value;
            }

            public static implicit operator Task(TaskNotifier notifier)
            {
                return notifier?._task;
            }
        }

        protected sealed class TaskNotifier<T> : ITaskNotifier<Task<T>>
        {
            internal TaskNotifier()
            {
            }

            private Task<T> _task;

            Task<T> ITaskNotifier<Task<T>>.Task
            {
                get => _task;
                set => _task = value;
            }

            public static implicit operator Task<T>(TaskNotifier<T> notifier)
            {
                return notifier?._task;
            }
        }
    }
}