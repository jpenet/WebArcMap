using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Tasks;
using System.Windows;
using ESRI.ArcGIS.Client.Geometry;

namespace Silverlight.Helper.DataMapping
{
	public class ResultsEventArgs : EventArgs
	{
		public FeatureSet Results { get; set; }
		public int State { get; set; }
		public ResultsEventArgs(FeatureSet results, int state)
		{
			this.Results = results;
			this.State = state;
		}
	}

	public class ResultInfoEventArgs : EventArgs
	{
		public Point ScreenPoint { get; set; }
		public MapPoint InfoPoint { get; set; }
		public IDictionary<string, Object> Attributes { get; set; }
		public string LayerId { get; set; }
		public bool Error { get; set; }
		public ResultInfoEventArgs(Point screenPoint, MapPoint infoPoint, bool error)
		{
			this.ScreenPoint = screenPoint;
			this.InfoPoint = infoPoint;
			this.Error = error;
			LayerId = string.Empty;
		}
	}

	public class ResultsListEventArgs : EventArgs
	{
		public List<FeatureSet> ResultsList { get; set; }
		public int State { get; set; }
		public ResultsListEventArgs(List<FeatureSet> resultsList, int state)
		{
			this.ResultsList = resultsList;
			this.State = state;
		}
	}
}
