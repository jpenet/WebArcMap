using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;
using System.Reflection;
using System.Linq;

namespace Silverlight.UI.Esri.JTMap.Helper
{
	public class RightClickMenu : TriggerAction<FrameworkElement>
	{
		// Definition of dependency properties that could be usefull
		public static readonly DependencyProperty CommandParameterProperty =
		DependencyProperty.Register("CommandParameter", typeof(object), typeof(RightClickMenu), null);

		public static readonly DependencyProperty CommandProperty =
		DependencyProperty.Register("Command", typeof(ICommand), typeof(RightClickMenu), null);

		public static readonly DependencyProperty InvokeParameterProperty = DependencyProperty.Register(
		"InvokeParameter", typeof(object), typeof(RightClickMenu), null);

		public object InvokeParameter
		{
			get
			{
				return this.GetValue(InvokeParameterProperty);
			}
			set
			{
				this.SetValue(InvokeParameterProperty, value);
			}
		}

		public ICommand Command
		{
			get
			{
				return (ICommand)this.GetValue(CommandProperty);
			}
			set
			{
				this.SetValue(CommandProperty, value);
			}
		}

		public object CommandParameter
		{
			get
			{
				return this.GetValue(CommandParameterProperty);
			}
			set
			{
				this.SetValue(CommandParameterProperty, value);
			}
		}

		private string commandName;
		public string CommandName
		{
			get
			{
				return this.commandName;
			}
			set
			{
				if (this.CommandName != value)
				{
					this.commandName = value;
				}
			}
		}


		protected override void Invoke(object parameter)
		{
			// Verify if it is a map
			MouseButtonEventArgs eventArgs = parameter as MouseButtonEventArgs;
			// Stop the mouse handling to eliminate the silverlight notice. This happen on the MouseRightButtonUp
			eventArgs.Handled = true;
			Map map = this.AssociatedObject as Map;
			if (map != null)
			{
				MapPoint mapPoint = map.ScreenToMap(eventArgs.GetPosition(map));
				ICommand command = this.ResolveCommand();
				if ((command != null) && command.CanExecute(this.CommandParameter))
				{
					command.Execute(mapPoint);
				}
			}
			return;
		}

		protected override void OnAttached()
		{
			return;
		}

		private ICommand ResolveCommand()
		{
			ICommand command = null;
			if (this.Command != null)
			{
				return this.Command;
			}
			// In the case the command is not defined within the triggger and you need to activate it, 
			// search is done for the context containing the command.
			var frameworkElement = this.AssociatedObject as FrameworkElement;
			if (frameworkElement != null)
			{
				object dataContext = frameworkElement.DataContext;
				if (dataContext != null)
				{
					PropertyInfo commandPropertyInfo = dataContext
					.GetType()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.FirstOrDefault(
					p =>
					typeof(ICommand).IsAssignableFrom(p.PropertyType) &&
					string.Equals(p.Name, this.CommandName, StringComparison.Ordinal)
					);

					if (commandPropertyInfo != null)
					{
						command = (ICommand)commandPropertyInfo.GetValue(dataContext, null);
					}
				}
			}
			return command;
		}
	}
}
