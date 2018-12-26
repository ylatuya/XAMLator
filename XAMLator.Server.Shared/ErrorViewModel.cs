//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace XAMLator.Server
{
	public class ErrorViewModel : INotifyPropertyChanged
	{
		private string _title;
		private string _error;
		private ICommand _closeCommand;

		public event PropertyChangedEventHandler PropertyChanged;

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				EmitPropertyChanged(nameof(Title));
			}
		}

		public string Error
		{
			get => _error;
			set
			{
				_error = value;
				EmitPropertyChanged(nameof(Error));
			}
		}

		public ICommand CloseCommand
		{
			get => _closeCommand;
			set
			{
				_closeCommand = value;
				EmitPropertyChanged(nameof(CloseCommand));
			}
		}

		public void SetError(string title, string ex)
		{
			Error = ex;
			Title = title;
		}

		public void SetError(string title, Exception ex)
		{
			SetError(title, ex.ToString());
		}

		public void SetError(string title, EvalResult result)
		{
			if (result.Messages.Length > 0)
			{
				Error = result.Messages[0].Text;
			}
			else
			{
				Error = "Unknown error";
			}
			Title = title;
		}

		void EmitPropertyChanged(string v)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
		}
	}
}
