//===================================================================================
// Authentication service. 
// Base on a user identification we will handle the authentication and user credentials for this login from a user.
// The authentication service is initated from the authentication module. All other modules are set depended to ensure
// that no processing occurs before authentication is succefull.
//===================================================================================
using System.ComponentModel;
using System.ComponentModel.Composition;
using Silverlight.Helper.Interfaces;
//using Silverlight.Services.General.AuthenticationService;

namespace Silverlight.Services.General
{
	[Export(typeof(IAuthentication))]
	public class Authentication : IAuthentication
	{
		private readonly string userName;
		private bool loggedOn = false;
		//private bool authorized = false;

		public Authentication()
		{
			userName = "Test User";
		}
		public bool IsLoggedOn()
		{
			return loggedOn;
		}

		public string UserName()
		{
			return userName;
		}

		public bool IsActionAllowed(string operation)
		{
			return true;
		}

		public bool IsActionAllowed(Helper.Enum.CRUD crud, string table)
		{
			return true;
		}


		public bool LogOn(string username, string password)
		{
			loggedOn = username.Equals(password);
			return loggedOn;
		}

		public void VerifyAuthorization()
		{
			//
			// get user credetntials
			//
			//AuthenticationServiceClient authenticationService = GetAuthenticationServiceClient();
			//AuthenticationServiceClient authenticationService = new AuthenticationServiceClient();
			//authenticationService.AccessAllowedCompleted += (s, ea) =>
			//{
			//  authorized = ea.Result;
			//  int status = 0;
			//  DoWorkEventArgs eventArgs = new DoWorkEventArgs(status);
			//  eventArgs.Result = ea.Result;
			//  OnFinishedVerify(eventArgs);
			//};
			//authenticationService.AccessAllowedAsync();
			//authorized = true;
			int status = 0;
			DoWorkEventArgs eventArgs = new DoWorkEventArgs(status);
			eventArgs.Result = true;
			OnFinishedVerify(eventArgs);
		}

		private event FinishedValidateHandeler finishedValidateAuthorization;
		public void SetFinishedEvent(FinishedValidateHandeler finishedValidateAuthorization)
		{
			this.finishedValidateAuthorization += finishedValidateAuthorization;
		}

		public void ResetFinishedEvent(FinishedValidateHandeler finishedValidateAuthorization)
		{
			this.finishedValidateAuthorization -= finishedValidateAuthorization;
		}

		/// <summary>
		/// Trigger the async finished event
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnFinishedVerify(DoWorkEventArgs e)
		{
			if (this.finishedValidateAuthorization != null)
			{
				this.finishedValidateAuthorization(this, e);
			}
		}

		//public static AuthenticationServiceClient GetAuthenticationServiceClient()
		//{
		//  BasicHttpBinding binding = new BasicHttpBinding(
		//  Application.Current.Host.Source.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)
		//  ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None);
		//  binding.MaxReceivedMessageSize = int.MaxValue;
		//  binding.MaxBufferSize = int.MaxValue;
		//  Uri uri = new Uri(Application.Current.Host.Source, "../Services/Authentication.svc");
		//  AuthenticationServiceClient result = new AuthenticationServiceClient(binding, new EndpointAddress(
		//  new Uri(Application.Current.Host.Source, "../Services/Authentication.svc")));
		//  return result;
		//}
	}
}
