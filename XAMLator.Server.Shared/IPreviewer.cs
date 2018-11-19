//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XAMLator.Server
{
	public interface IPreviewer
	{
		ICommand CloseCommand { get; }

		Task Preview(EvalResult res);

		Task NotifyError(ErrorViewModel errorViewModel);
	}
}
