namespace Silverlight.Helper.DataMapping
{
	public class LayerData
	{
		public string LayerName { get; set; }
		public int ID { get; set; }
		public bool Selection { get; set; }
		public int SelectCount { get; set; }
		public string DisplayInfo
		{
			get
			{
				if (SelectCount == 0)
				{
					return LayerName;
				}
				else
				{
					return LayerName + SelectCount.ToString("(##0)");
				}
			}
		}
	}
}
