using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace XAMLator
{
	public class EvalRequest
	{
		public string Declarations;
		public string ValueExpression;
		public string Xaml;
		public string XamlType;
	}

	public class EvalMessage : INotifyPropertyChanged
	{
		public string MessageType;
		public string Text;
		public int Line;
		public int Column;

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class EvalResult
	{
		public EvalMessage [] Messages;
		public TimeSpan Duration;
		public object Result;
		public bool HasResult;
		public string Xaml;

		public bool HasErrors {
			get { return Messages != null && Messages.Any (m => m.MessageType == "error"); }
		}
	}

	public class EvalResponse
	{
		public EvalMessage [] Messages;
		public Dictionary<string, List<string>> WatchValues;
		public TimeSpan Duration;

		public bool HasErrors {
			get { return Messages != null && Messages.Any (m => m.MessageType == "error"); }
		}
	}
}

