using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Practices.Prism.ViewModel;

namespace Silverlight.Helper.ViewModels
{
	public abstract class ObjectBase : NotificationObject, IDataErrorInfo
	{
		protected bool _IsDirty = false;

		//public event PropertyChangedEventHandler PropertyChanged;

		public virtual bool IsDirty
		{
			get
			{
				return _IsDirty;
			}
			set
			{
				_IsDirty = value;
				RaisePropertyChanged("IsDirty", false);
			}
		}

		protected override void RaisePropertyChanged(string propertyName)
		{
			RaisePropertyChanged(propertyName, true);
		}

		protected virtual void RaisePropertyChanged(string propertyName, bool dirty)
		{
			if (dirty)
			{
				_IsDirty = true;
				base.RaisePropertyChanged("IsDirty");
			}
			base.RaisePropertyChanged(propertyName);
		}

		protected ObservableCollection<string> _InvalidFields = new ObservableCollection<string>();

		public ObservableCollection<string> InvalidFields
		{
			get
			{
				return _InvalidFields;
			}
		}

		public bool IsValid
		{
			get
			{
				return (_InvalidFields.Count == 0);
			}
		}

		protected virtual string ValidateProperty(string propertyName)
		{
			return string.Empty;
		}

		protected virtual string PostValidateProperty(string propertyName, string error)
		{
			return string.Empty;
		}

		string IDataErrorInfo.Error
		{
			get
			{
				string ret = string.Empty;
				string verb = (InvalidFields.Count == 1 ? "is" : "are");
				string suffix = (InvalidFields.Count == 1 ? "" : "s");

				if (!IsValid)
					ret = string.Format("There {0} {1} validation error{2}.", verb, InvalidFields.Count, suffix);

				return ret;
			}
		}

		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				string error = ValidateProperty(columnName);

				if (error != string.Empty)
				{
					if (!InvalidFields.Contains(columnName))
						InvalidFields.Add(columnName);
				}
				else
				{
					if (InvalidFields.Contains(columnName))
						InvalidFields.Remove(columnName);
				}

				error = PostValidateProperty(columnName, error);

				return error;
			}
		}

		protected virtual void UpdateValidationBindings()
		{
		}
	}
}
