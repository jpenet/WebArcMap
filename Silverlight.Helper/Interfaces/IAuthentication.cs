using System;
using Silverlight.Helper.Enum;
using System.ComponentModel;

namespace Silverlight.Helper.Interfaces
{
	public delegate void FinishedValidateHandeler(object sender, DoWorkEventArgs args);
	public interface IAuthentication
	{
		Boolean IsLoggedOn();
		string UserName();
		Boolean IsActionAllowed(string operation);
		Boolean IsActionAllowed(CRUD crud, string table);
		Boolean LogOn(string username, string password);
		void VerifyAuthorization();
		void SetFinishedEvent(FinishedValidateHandeler finishedValidateAuthorization);
		void ResetFinishedEvent(FinishedValidateHandeler finishedValidateAuthorization);
	}
}
