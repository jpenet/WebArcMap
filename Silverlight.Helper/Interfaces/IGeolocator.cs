using System.Collections.Generic;
using Silverlight.Helper.DataMapping;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace Silverlight.Helper.Interfaces
{
	public delegate void ResultsGeoLocatorHandler(object sender, ResultsGeoLocatorEventArgs e);
	public interface IGeolocator
	{
		void Initialize(string urlGeoLocatorService);
		void SetResultsGeoLocatorHandler(ResultsGeoLocatorHandler resultsGeoLocatorHandler);
		void SetLocationMarker(MarkerSymbol locationMarker);
		MarkerSymbol GetLocationMarker();
		void SetSpatialReference(SpatialReference spatialReference);
		void FindAddress(string streetName, string streetNumber, string postCode, string city);
		void FindAddressByLocation(MapPoint location);
	}
}
