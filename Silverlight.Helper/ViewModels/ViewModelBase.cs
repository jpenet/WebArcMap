using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Silverlight.Helper.ViewModels
{
	public abstract class ViewModelBase : ObjectBase
	{
		protected bool _SubmitAttempted = false;
		bool _LateValidate = false;

		public bool LateValidate
		{
			get
			{
				return _LateValidate;
			}
			set
			{
				_LateValidate = value;
			}
		}

		protected virtual void InitializeViewModel()
		{
			TransferFromModel();
		}

		protected virtual void TransferFromModel()
		{
		}

		protected virtual void TransferToModel()
		{
		}

		public abstract string ViewName { get; }

		protected virtual void RaiseCommandsCanExecute()
		{
		}
		protected override string PostValidateProperty(string propertyName, string error)
		{
			if (LateValidate)
			{
				if (!_SubmitAttempted)
					error = string.Empty;
			}
			RaiseCommandsCanExecute();

			return error;
		}

		protected override void UpdateValidationBindings()
		{
			// properties that have to do with possible validation display
			//OnPropertyChanged("InvalidFields", false);
		}
	}
}
