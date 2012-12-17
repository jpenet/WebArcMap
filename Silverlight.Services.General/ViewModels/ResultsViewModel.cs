using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Dialogs;
using Silverlight.Helper.Interfaces;

namespace Silverlight.Services.General.ViewModels
{
	public class ResultsViewModel : NotificationObject
	{
		public struct AttributeItem
		{
			public string FieldName { get; set; }
			public string FieldValue { get; set; }
		}

		private ICommand _okCommand;
		private ObservableCollection<SearchResult> _displayResults;
		private ObservableCollection<AttributeItem> _attributeValues;

		[Import]
		public IMessageBoxCustom messageBoxCustom;

		public ObservableCollection<AttributeItem> AttributeValues
		{
			get
			{
				return _attributeValues;
			}
			set
			{
				_attributeValues = value;
			}
		}

		public ObservableCollection<SearchResult> DisplayResults
		{
			get
			{
				return _displayResults;
			}
			set
			{
				_displayResults = value;
				this.RaisePropertyChanged("DisplayResults");
			}
		}

		private string _LayerName;

		public string LayerName
		{
			get
			{
				return _LayerName;
			}
			set
			{
				_LayerName = value;
				this.RaisePropertyChanged(() => this.LayerName);
			}
		}

		public ICommand OkCommand
		{
			get
			{
				return _okCommand;
			}
			set
			{
				_okCommand = value;
				this.RaisePropertyChanged(() => this.OkCommand);
			}
		}

		public ResultsViewModel()
		{
			_okCommand = new DelegateCommand<object>(OnOkCommandClicked, CanOkCommand);
		}

		public void SetResults(List<SearchResult> results)
		{
			if (results == null || results.Count == 0)
			{
				var result = messageBoxCustom.Show("No results where found for the query.", "Remark",
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
			this.DisplayResults = new ObservableCollection<SearchResult>(results);
			_attributeValues = new ObservableCollection<AttributeItem>();
			if (results[0].AttributeValues.Count > 0)
			{
				foreach (var item in results[0].AttributeValues)
				{
					string value = string.Empty;
					if (item.Value != null)
						value = item.Value.ToString();
					AttributeItem attrib = new AttributeItem();
					attrib.FieldValue = value;
					attrib.FieldName = item.Key;
					this._attributeValues.Add(attrib);
				}
				this.LayerName = results[0].LayerName;
				this.RaisePropertyChanged("AttributeValues");
			}
			else
			{
				var result = messageBoxCustom.Show("No attributes for this feature.", "Remark",
				MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		private void OnOkCommandClicked(object arg)
		{
		}

		private bool CanOkCommand(object arg)
		{
			return true;
		}
	}
}
