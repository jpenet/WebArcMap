//===================================================================================
// Routing service based on the Bin map API
//===================================================================================
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Bing.RouteService;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Projection;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Silverlight.Helper.DataMapping;
using Silverlight.Helper.Dialogs;
using Silverlight.Helper.Interfaces;
using Silverlight.Services.Gis.Resources;

namespace Silverlight.Services.Gis
{
	[Export(typeof(IGisRouting))]
	public class GisRouting : IGisRouting
	{
		private readonly IGisOperations gisOperations;
		private GraphicsLayer graphicsRouteLayer = null;
		private readonly Routing routing;
		private readonly static ESRI.ArcGIS.Client.Projection.WebMercator mercator = new ESRI.ArcGIS.Client.Projection.WebMercator();
		private readonly List<MapPoint> pointsSelected = new List<MapPoint>();
		private RouteTask routeTask;
		private SimpleMarkerSymbol stopRouteSymbol;
		private SimpleMarkerSymbol wayPointRouteSymbol;
		private SimpleLineSymbol lineRouteSymbol;
		private readonly IMessageBoxCustom messageBoxCustom;
		/// <summary>
		/// Make the link to the more fine gis operations, intialise the Bingmap service
		/// </summary>
		/// <param name="gisOperations"></param>
		public GisRouting(IGisOperations gisOperations, IMessageBoxCustom messageBoxCustom)
		{
			this.gisOperations = gisOperations;
			routing = new Routing(BingMapKey);
			this.messageBoxCustom = messageBoxCustom;
		}

		private const string BingMapKey = "Aq99fQnVFPwEEwKksd1IrIP2sC659MdRsWrdL0pPfWSQhLa9Q1T8w4K4e54SBq0H";

		public void Initialize(string URLRouting)
		{
			routeTask = new RouteTask(URLRouting);
		}

		#region ArcGis Routing functionality
		public void StartRouteCalculation(RouteParameters routeParameters, string routeName)
		{
			routeParameters.DirectionsLanguage = new CultureInfo("nl-NL");
			routeParameters.ReturnDirections = true;
			routeParameters.FindBestSequence = true;
			routeParameters.PreserveFirstStop = true;
			routeParameters.PreserveLastStop = true;
			routeParameters.ReturnStops = true;
			routeParameters.DirectionsLengthUnits = esriUnits.esriKilometers;
			routeTask.Failed += (s, a) =>
			{
				string errorMessage = "Routing error: ";
				errorMessage += a.Error.Message;
				foreach (string detail in (a.Error as ServiceException).Details)
					errorMessage += "," + detail;
				messageBoxCustom.Show(errorMessage, "Error");
			};
			routeTask.SolveCompleted += RouteSolvedComplete;
			routeTask.SolveAsync(routeParameters, routeName);
		}

		private void RouteSolvedComplete(object sender, RouteEventArgs a)
		{
			try
			{
				routeTask.SolveCompleted -= RouteSolvedComplete;
				WebMercator webMercator = new WebMercator();
				RoutingEventArgs routingEvent = new RoutingEventArgs();
				ESRI.ArcGIS.Client.Tasks.RouteResult routeResult = a.RouteResults[0];
				routeResult.Route.Symbol = lineRouteSymbol;
				routingEvent.RoutingResult.RouteGraphic = routeResult.Route;
				SpatialReference initialSpatialReference =
				routingEvent.RoutingResult.RouteGraphic.Geometry.SpatialReference;
				routingEvent.RoutingResult.RouteGraphic.Geometry =
				webMercator.FromGeographic(routeResult.Route.Geometry);
				routingEvent.RoutingResult.RouteName = a.UserState as string;
				routingEvent.RoutingResult.RouteDirections = routeResult.Directions;
				foreach (var item in routingEvent.RoutingResult.RouteDirections)
				{
					item.Geometry.SpatialReference = initialSpatialReference;
					item.Geometry = webMercator.FromGeographic(item.Geometry);
				}
				OnRoutingCalcComplete(routingEvent);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("RouteSolvedComplete /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Get the route  marker symbol for selection
		/// </summary>
		/// <returns></returns>
		public SimpleMarkerSymbol GetStopRouteSymbol()
		{
			return stopRouteSymbol;
		}

		/// <summary>
		/// Get the route line symbol for selection
		/// </summary>
		/// <returns></returns>
		public SimpleLineSymbol GetLineRouteSymbol()
		{
			return lineRouteSymbol;
		}

		public SimpleMarkerSymbol GetWayPointRouteSymbol()
		{
			return wayPointRouteSymbol;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="symbol"></param>
		public void SetStopRouteSymbol(SimpleMarkerSymbol symbol)
		{
			stopRouteSymbol = symbol;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="symbol"></param>
		public void SetLineRouteSymbol(SimpleLineSymbol symbol)
		{
			lineRouteSymbol = symbol;
		}

		public void SetWayPointRouteSymbol(SimpleMarkerSymbol symbol)
		{
			wayPointRouteSymbol = symbol;
		}
		#endregion

		#region Bingmap routing

		/// <summary>
		/// Set the callback function from the caller for the routing calculation
		/// </summary>
		/// <param name="finishedOperation"></param>
		public void SetFinishedEvent(RoutingHandler finishedOperation)
		{
			routingComplete += finishedOperation;
		}

		/// <summary>
		/// reset the callback function from the caller for the routing calculation
		/// </summary>
		/// <param name="finishedOperation"></param>
		public void ResetFinishedEvent(RoutingHandler finishedOperation)
		{
			routingComplete -= finishedOperation;
		}

		/// <summary>
		/// Create the waypoints for the route
		/// </summary>
		/// <param name="graphicLayerName"></param>
		/// <returns></returns>
		public bool CreateRouting(string graphicLayerName)
		{
			try
			{
				//Step 1 - Set points 
				pointsSelected.Clear();
				graphicsRouteLayer = gisOperations.GetFeatureLayer(graphicLayerName);
				graphicsRouteLayer.ClearGraphics();
				// Handle points
				gisOperations.SetCompleteDrawEvent(DrawComplete);
				gisOperations.SetDrawModeContinuous(DrawMode.Point);
				return true;
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("CreateRouting /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
				return false;
			}
		}

		/// <summary>
		/// Handle the drawcomplete for a point
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void DrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
		{
			try
			{
				Geometry geometry =
				gisOperations.GetMap().WrapAroundIsActive ?
				Geometry.NormalizeCentralMeridian(args.Geometry) : args.Geometry;
				MapPoint point = geometry as MapPoint;
				pointsSelected.Add(new MapPoint(point.X, point.Y, point.SpatialReference));
				// Display point
				Graphic graphic = new Graphic()
				{
					Geometry = point,
					Symbol = this.stopRouteSymbol as Symbol
				};
				graphic.Attributes.Add("Stop", pointsSelected.Count);
				graphicsRouteLayer.Graphics.Add(graphic);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("DrawComplete /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// calculate the route async for the waypoint saved.
		/// </summary>
		/// <returns></returns>
		public bool CalculateRouting()
		{
			if (pointsSelected.Count == 0)
				return false;
			// Start processing route
			gisOperations.DisableDrawMode();
			routing.Optimization = RouteOptimization.MinimizeTime;
			routing.TrafficUsage = TrafficUsage.None;
			routing.TravelMode = TravelMode.Driving;
			routing.Route(pointsSelected, Route_Complete);
			return true;
		}

		/// <summary>
		/// Event executed when the calculation is finished.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void Route_Complete(object sender, CalculateRouteCompletedEventArgs args)
		{
			try
			{
				gisOperations.EnableDrawMode();
				RoutingEventArgs routeArgs = new RoutingEventArgs();
				List<string> directions = new List<string>();
				if (args.Error != null)
				{
					directions.Add(args.Error.Message);
					routeArgs.RoutingResult.Directions = directions;
				}
				else
				{
					ObservableCollection<RouteLeg> routeLegs = args.Result.Result.Legs;
					int numLegs = routeLegs.Count;
					int instructionCount = 0;
					for (int n = 0; n < numLegs; n++)
					{
						directions.Add(string.Format("--Leg #{0}--\n", n + 1));

						foreach (ItineraryItem item in routeLegs[n].Itinerary)
						{
							instructionCount++;
							directions.Add(string.Format("{0}. {1}\n", instructionCount, item.Text));
						}
					}

					Regex regex = new Regex("<[/a-zA-Z:]*>",
					RegexOptions.IgnoreCase | RegexOptions.Multiline);

					RoutePath routePath = args.Result.Result.RoutePath;

					Polyline line = new Polyline();
					line.Paths.Add(new PointCollection());

					foreach (Location location in routePath.Points)
						line.Paths[0].Add(mercator.FromGeographic(new MapPoint(location.Longitude, location.Latitude)) as MapPoint);

					Graphic graphic = new Graphic()
					{
						Geometry = line,
						Symbol = this.lineRouteSymbol as Symbol
					};
					graphicsRouteLayer.Graphics.Add(graphic);


					routeArgs.RoutingResult.Directions = directions;
					routeArgs.RoutingResult.RouteLine = line;
				}
				gisOperations.DisableDrawMode();
				OnRoutingCalcComplete(routeArgs);

			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("Route_Complete /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		/// <summary>
		/// Get the Binglayer that must be added to the map
		/// </summary>
		/// <returns></returns>
		public Layer GetBingLayer()
		{
			TileLayer bingLayer = new TileLayer() { ID = "Routes",
			Token = BingMapKey,
			ServerType = ServerType.Production,
			Visible = false };
			return bingLayer;
		}
		#endregion

		#region Common routing methods
		private event RoutingHandler routingComplete;
		/// <summary>
		/// Callback for the routing calculation
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRoutingCalcComplete(RoutingEventArgs e)
		{
			if (routingComplete != null)
			{
				routingComplete(this, e);
			}
		}

		/// <summary>
		/// Clear the routing graphics
		/// </summary>
		public void ClearRouting()
		{
			if (graphicsRouteLayer != null)
			{
				graphicsRouteLayer.ClearGraphics();
			}
		}

		#endregion
	}
}
