//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace XAMLator.Server
{
	public class ErrorViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string title;
		string error;

		public ErrorViewModel (string title, Exception ex)
		{
			this.error = ex.ToString ();
			this.title = title;
		}

		public ErrorViewModel (string title, EvalResult result)
		{
			error = result.Messages [0].Text;
			this.title = title;
		}

		public string Title {
			get => title;
		}

		public string Error {
			get => error;
		}

		void EmitPropertyChanged (string v)
		{
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (v));
		}
	}
}
