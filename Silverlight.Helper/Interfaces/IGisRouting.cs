using ESRI.ArcGIS.Client;
using Silverlight.Helper.DataMapping;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace Silverlight.Helper.Interfaces
{
	public delegate void RoutingHandler(object sender, RoutingEventArgs e);
	public interface IGisRouting
	{
		SimpleMarkerSymbol GetStopRouteSymbol();
		SimpleLineSymbol GetLineRouteSymbol();
		void SetStopRouteSymbol(SimpleMarkerSymbol symbol);
		void SetLineRouteSymbol(SimpleLineSymbol symbol);
		void SetFinishedEvent(RoutingHandler finishedOperation);
		void ResetFinishedEvent(RoutingHandler finishedOperation);
		bool CreateRouting(string graphicLayerName);
		void ClearRouting();
		bool CalculateRouting();
		Layer GetBingLayer();
		void Initialize(string URLRouting);
		void StartRouteCalculation(RouteParameters routeParameters, string routeName);
		SimpleMarkerSymbol GetWayPointRouteSymbol();
		void SetWayPointRouteSymbol(SimpleMarkerSymbol symbol);
	}
}
