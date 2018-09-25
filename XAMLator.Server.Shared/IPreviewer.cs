//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using System.Threading.Tasks;

namespace XAMLator.Server
{
	public interface IPreviewer
	{
		Task Preview (EvalResult res);

		Task NotifyError (ErrorViewModel errorViewModel);
	}
}
