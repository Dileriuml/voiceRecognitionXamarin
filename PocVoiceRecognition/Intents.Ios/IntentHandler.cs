using System;

using Foundation;
using Intents;

namespace CanIIntents
{
	// As an example, this class is set up to handle Message intents.
	// You will want to replace this or add other intents as appropriate.
	// The intents you wish to handle must be declared in the extension's Info.plist.

	// You can test your example integration by saying things to Siri like:
	// "Send a message using <myApp>"
	// "<myApp> John saying hello"
	// "Search for messages in <myApp>"
	[Register("IntentHandler")]
	public class IntentHandler : INExtension, IINSendMessageIntentHandling
	{
		protected IntentHandler(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override NSObject GetHandler(INIntent intent)
		{
			// This is the default implementation.  If you want different objects to handle different intents,
			// you can override this and return the handler you want for that particular intent.

			return this;
		}

		[Export("resolveContentForSendMessage:withCompletion:")]
		public void ResolveContent(INSendMessageIntent intent, Action<INStringResolutionResult> completion)
		{
			var text = intent.Content;
			if (!string.IsNullOrEmpty(text))
				completion(INStringResolutionResult.GetSuccess(text));
			else
				completion(INStringResolutionResult.NeedsValue);
		}

		// Once resolution is completed, perform validation on the intent and provide confirmation (optional).
		[Export("confirmSendMessage:completion:")]
		public void ConfirmSendMessage(INSendMessageIntent intent, Action<INSendMessageIntentResponse> completion)
		{
			// Verify user is authenticated and your app is ready to send a message.

			var userActivity = new NSUserActivity("INSendMessageIntent");
			var response = new INSendMessageIntentResponse(INSendMessageIntentResponseCode.Ready, userActivity);
			completion(response);
		}

		// Handle the completed intent (required).
		public void HandleSendMessage(INSendMessageIntent intent, Action<INSendMessageIntentResponse> completion)
		{
			// Implement your application logic to send a message here.

			var userActivity = new NSUserActivity("com.trinetix.handlemessage");

			// Define details
			var info = new NSMutableDictionary();
			info.Add(new NSString("message"), new NSString(intent.Content));

			// Populate Activity
			userActivity.Title = "CanI Request";
			userActivity.UserInfo = info;

			// Add App Search ability
			userActivity.EligibleForHandoff = true;
			userActivity.EligibleForSearch = true;
			userActivity.EligibleForPublicIndexing = true;
			userActivity.BecomeCurrent();

			// Assemble response and send it
			var response = new INSendMessageIntentResponse(INSendMessageIntentResponseCode.InProgress, userActivity);
			completion(response);
		}
	}
}
