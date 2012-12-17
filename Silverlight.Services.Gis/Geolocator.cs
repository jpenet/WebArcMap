//===================================================================================
// Geolocator service.
// Initiated in the bootstrapper. The only important parameter is the URL of the geolocator.
// The URL is retrieved from the configuration file, and is passed to the service during the
// initialize of the Map
//===================================================================================
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
	[Export(typeof(IGeolocator))]
	public class Geolocator : IGeolocator
	{
		private string urlGeolocatorService;
		private const int MaxResult = 40; // Maximum allowed
		private MarkerSymbol locationMarker; // Location symbol used for the marker
		private SpatialReference spatialReference;
		private readonly IMessageBoxCustom messageBoxCustom;
		private Locator locatorTask;

		public Geolocator(IMessageBoxCustom messageBoxCustom)
		{
			this.messageBoxCustom = messageBoxCustom;
		}

		/// <summary>
		/// Save the URL for use in all geolocator method calls
		/// </summary>
		/// <param name="urlGeoLocatorService"></param>
		public void Initialize(string urlGeoLocatorService)
		{
			this.urlGeolocatorService = urlGeoLocatorService;
			locatorTask = new Locator(urlGeolocatorService);			
		}

		public void SetLocationMarker(MarkerSymbol locationMarker)
		{
			this.locationMarker = locationMarker;
		}

		public MarkerSymbol GetLocationMarker()
		{
			return locationMarker;
		}

		public void SetSpatialReference(SpatialReference spatialReference)
		{
			this.spatialReference = spatialReference;
		}

		/// <summary>
		/// Trigger the finished event for the async operation
		/// </summary>
		private ResultsGeoLocatorHandler resultsGeoLocatorHandler;
		private void OnGeoLocatorSearchComplete(ResultsGeoLocatorEventArgs e)
		{
			if (resultsGeoLocatorHandler != null)
				resultsGeoLocatorHandler(this, e);
		}

		/// <summary>
		/// Set the handler for the async operation.
		/// </summary>
		/// <param name="resultsGeoLocatorHandler"></param>
		public void SetResultsGeoLocatorHandler(ResultsGeoLocatorHandler resultsGeoLocatorHandler)
		{
			this.resultsGeoLocatorHandler = resultsGeoLocatorHandler;
		}


		public void FindAddress(string streetName, string streetNumber, string postCode, string city)
		{
			try
			{
				locatorTask.AddressToLocationsCompleted +=
				locatorTask_AddressToLocationsCompleted;
				locatorTask.Failed += locatorTask_Failed;
				// Set parameters
				AddressToLocationsParameters addressParameters = new AddressToLocationsParameters();
				Dictionary<string, string> address = addressParameters.Address;
				//
				address.Add("Address", String.Format("{0} {1}", streetName, streetNumber));
				if (city.Length > 0)
					address.Add("City", city);
				address.Add("Country", "BE");
				if (postCode.Length > 0)
					address.Add("Postcode", postCode);
				locatorTask.AddressToLocationsAsync(addressParameters);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("FindAddress /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		void locatorTask_Failed(object sender, TaskFailedEventArgs e)
		{
			try
			{
				locatorTask.AddressToLocationsCompleted -=
					locatorTask_AddressToLocationsCompleted;
				IList<GeoLocatorDetail> result = new List<GeoLocatorDetail>();
				result.Add(new GeoLocatorDetail()
				{
					Location = null,
					Match = 0.0,
					Title = e.Error.Message
				});
				if (e.Error.InnerException != null)
					result[0].Title = String.Format("{0}-{1}", result[0].Title, e.Error.InnerException.Message);
				ResultsGeoLocatorEventArgs resultEventArgs = new ResultsGeoLocatorEventArgs(result);
				OnGeoLocatorSearchComplete(resultEventArgs);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("locatorTask_Failed /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		void locatorTask_AddressToLocationsCompleted(object sender, AddressToLocationsEventArgs e)
		{
			try
			{
				locatorTask.AddressToLocationsCompleted -=
				locatorTask_AddressToLocationsCompleted;
				IList<GeoLocatorDetail> result = new List<GeoLocatorDetail>();
				WebMercator webMercator = new WebMercator();
				if (e.Results.Count > 0)
				{
					foreach (var item in e.Results)
					{
						result.Add(new GeoLocatorDetail()
						{
							Location = webMercator.FromGeographic(item.Location) as MapPoint,
							Match = System.Convert.ToDouble(item.Score),
							Title = item.Address
						});
					}
				}
				else
				{
				}
				ResultsGeoLocatorEventArgs resultEventArgs = new ResultsGeoLocatorEventArgs(result);
				OnGeoLocatorSearchComplete(resultEventArgs);

			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("locatorTask_AddressToLocationsCompleted /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}

		public void FindAddressByLocation(MapPoint location)
		{
			locatorTask.LocationToAddressCompleted +=
			locatorTask_LocationToAddressCompleted;
			locatorTask.Failed += locatorTask_Failed;
			locatorTask.LocationToAddressAsync(location, 10);
		}
		/// <summary>
		///
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void locatorTask_LocationToAddressCompleted(object sender, AddressEventArgs e)
		{
			try
			{
				locatorTask.LocationToAddressCompleted -= locatorTask_LocationToAddressCompleted;
				Address address = e.Address;
				Dictionary<string, object> attributes = e.Address.Attributes;
				IList<GeoLocatorDetail> result = new List<GeoLocatorDetail>();
				WebMercator webMercator = new WebMercator();
				result.Add(new GeoLocatorDetail()
				{
					Location = address.Location,
					Match = 100.0,
					Title = string.Format("{0} {1},{2}", attributes["Address"], attributes["City"], attributes["Postcode"]),
					Attributes = attributes
				});
				ResultsGeoLocatorEventArgs resultEventArgs = new ResultsGeoLocatorEventArgs(result);
				OnGeoLocatorSearchComplete(resultEventArgs);
			}
			catch (Exception ex)
			{
				messageBoxCustom.Show(String.Format("locatorTask_LocationToAddressCompleted /{0}", ex.Message),
					GisTexts.SevereError, 
					MessageBoxCustomEnum.MessageBoxButtonCustom.Ok);
			}
		}
	}
}
