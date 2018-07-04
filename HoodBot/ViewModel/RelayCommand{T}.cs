﻿namespace RobinHood70.HoodBot.ViewModel
{
	using System;
	using System.Windows.Input;

	public class RelayCommand<T> : ICommand
	{
		private readonly Action<T> action;
		private readonly Predicate<T> canExecute;

		public RelayCommand(Action<T> action) => this.action = action;

		public RelayCommand(Action<T> action, Predicate<T> canExecute)
			: this(action) => this.canExecute = canExecute;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(T parameter) => this.canExecute == null ? true : this.canExecute(parameter);

		public bool CanExecute(object parameter) => this.CanExecute(parameter is null && typeof(T).IsValueType ? default : (T)parameter);

		public void Execute(T parameter) => this.action(parameter);

		public void Execute(object parameter) => this.Execute(parameter is null && typeof(T).IsValueType ? default : (T)parameter);
	}
}