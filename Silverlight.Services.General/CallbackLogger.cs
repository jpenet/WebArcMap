//===================================================================================
// Microsoft patterns & practices
// Composite Application Guidance for Windows Presentation Foundation and Silverlight
//===================================================================================
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===================================================================================
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//===================================================================================
using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Logging;

namespace Silverlight.Services.General
{
	/// <summary>
	/// A logger that holds on to log entries until a callback delegate is set, then plays back log entries and sends new log entries.
	/// </summary>
	public class CallbackLogger : ILoggerFacade
	{
		private readonly Queue<Tuple<string, Category, Priority, DateTime, string, string>> savedLogs =
		new Queue<Tuple<string, Category, Priority, DateTime, string, string>>();
		private Action<string, Category, Priority, DateTime, string, string> callback;

		/// <summary>
		/// Gets or sets the callback to receive logs.
		/// </summary>
		/// <value>An Actionstring, Category, Priority callback.</value>
		public Action<string, Category, Priority, DateTime, string, string> Callback
		{
			get
			{
				return this.callback;
			}
			set
			{
				this.callback = value;
			}
		}
		/// <summary>
		/// Maximum number of items in the queue before action to been taken
		/// </summary>
		public int MaxSize { get; set; }

		public string Version { get; set; }
		/// <summary>
		/// Write a new log entry with the specified category and priority.
		/// </summary>
		/// <param name="message">Message body to log.</param>
		/// <param name="category">Category of the entry.</param>
		/// <param name="priority">The priority of the entry.</param>
		public void Log(string message, Category category, Priority priority)
		{
			if (this.Callback != null)
			{
				this.Callback(message, category, priority, DateTime.Now, string.Empty, this.Version);
			}
			else
			{
				this.savedLogs.Enqueue(new Tuple<string, Category, Priority, DateTime, string, string>(message, category, priority, DateTime.Now, string.Empty, this.Version));
				if (this.savedLogs.Count > this.MaxSize)
				{
					// Take action on the queue to be processed (towards the server)
				}
			}
		}
		/// <summary>
		/// Base log function
		/// </summary>
		/// <param name="message"></param>
		/// <param name="category"></param>
		/// <param name="priority"></param>
		/// <param name="className"></param>
		public void Log(string message, Category category, Priority priority, string className)
		{
			if (this.Callback != null)
			{
				this.Callback(message, category, priority, DateTime.Now, className, this.Version);
			}
			else
			{
				this.savedLogs.Enqueue(new Tuple<string, Category, Priority, DateTime, string, string>(message, category, priority, DateTime.Now, className, this.Version));
				if (this.savedLogs.Count > this.MaxSize)
				{
					// Take action on the queue to be processed (towards the server)
					// Use a WCF service to log the contents
				}
			}
		}
		/// <summary>
		/// Extented log function to be able to flush the error log contents. Can be used as wrapper for the logging
		/// </summary>
		/// <param name="message"></param>
		/// <param name="category"></param>
		/// <param name="priority"></param>
		/// <param name="className"></param>
		/// <param name="emptyLog"></param>
		public void Log(string message, Category category, Priority priority, string className, bool emptyLog)
		{
			if (emptyLog)
			{
				int oldMaxSize = this.MaxSize;
				this.MaxSize = 0;
				Log(message, category, priority, className);
				this.MaxSize = oldMaxSize;
			}
			else
				Log(message, category, priority, className);
		}

		/// <summary>
		/// Replays the saved logs if the Callback has been set.
		/// </summary>
		public void ReplaySavedLogs()
		{
			if (this.Callback != null)
			{
				while (this.savedLogs.Count > 0)
				{
					var log = this.savedLogs.Dequeue();
					this.Callback(log.Item1, log.Item2, log.Item3, log.Item4, log.Item5, log.Item6);
				}
			}
		}
	}
}
