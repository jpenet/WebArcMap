namespace Silverlight.Helper.Interfaces
{
	public delegate TStatus ReturnAction<TStatus, TParameter>(TParameter arg);
	public interface INavigateDialog<TStatus, TParameter>
	{
		void Display(ReturnAction<TStatus, TParameter> returnAction);
	}
}
