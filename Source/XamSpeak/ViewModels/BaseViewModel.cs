using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;

namespace XamSpeak
{
    abstract class BaseViewModel : INotifyPropertyChanged
    {
        readonly WeakEventManager _propertyChangedWeakEventManager = new WeakEventManager();

        bool _isInternetConnectionActive;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChangedWeakEventManager.AddEventHandler(value);
            remove => _propertyChangedWeakEventManager.RemoveEventHandler(value);
        }

        public bool IsInternetConnectionActive
        {
            get => _isInternetConnectionActive;
            set => SetProperty(ref _isInternetConnectionActive, value);
        }

        protected void SetProperty<T>(ref T backingStore, in T value, in Action? onChanged = null, [CallerMemberName] in string propertyname = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyname);
        }

        protected void OnPropertyChanged([CallerMemberName] in string name = "") =>
            _propertyChangedWeakEventManager.HandleEvent(this, new PropertyChangedEventArgs(name), nameof(INotifyPropertyChanged.PropertyChanged));
    }
}
