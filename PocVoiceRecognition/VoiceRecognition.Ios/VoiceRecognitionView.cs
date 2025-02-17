﻿using System;
using System.Threading.Tasks;
using Speech;
using UIKit;
using Foundation;
using AVFoundation;
using CoreSpotlight;
using MobileCoreServices;

namespace CanI.Ios
{
	public partial class VoiceRecognitionView : UIViewController
	{
		public AppDelegate ThisApp
		{
			get { return (AppDelegate)UIApplication.SharedApplication.Delegate; }
		}

		private AVAudioEngine audioEngine = new AVAudioEngine();
		private SFSpeechRecognizer speechRecognizer = new SFSpeechRecognizer();

		public VoiceRecognitionView() : base("VoiceRecognitionView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Register with app delegate
			ThisApp.Controller = this;

			RecognizeButton.TouchUpInside += RecognizeButtonTouchUpInside;
			RecognizeButton.Enabled = false;
			SFSpeechRecognizer.RequestAuthorization(HandleVoiceAuthorization);

			SetupUserActionSearch();

			SetupCoreSpotlightSearch();
		}

		private void SetupUserActionSearch()
		{
			// Create App Search Activity
			var activity = new NSUserActivity("com.xamarin.platform");

			// Define details
			var info = new NSMutableDictionary();
			info.Add(new NSString("link"), new NSString("http://xamarin.com/platform"));

			// Populate Activity
			activity.Title = "The Xamarin Platform";
			activity.UserInfo = info;

			// Add App Search ability
			activity.EligibleForSearch = true;
			activity.BecomeCurrent();
		}

		private void SetupCoreSpotlightSearch()
		{
			// Create attributes to describe an item
			var attributes = new CSSearchableItemAttributeSet(UTType.Data);
			attributes.Title = "Test Cloud";
			attributes.ContentDescription = "Automatically test your app on 1,000 devices in the cloud.";

			// Create item
			var item = new CSSearchableItem("1", "products", attributes);

			// Index item
			CSSearchableIndex.DefaultSearchableIndex.Index(new CSSearchableItem[] { item }, (error) =>
			{
				// Successful?
				if (error != null)
				{
					Console.WriteLine(error.LocalizedDescription);
				}
			});
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
		}

		private async void RecognizeButtonTouchUpInside(object sender, EventArgs e)
		{
			RecognizeButton.Enabled = false;

			var recognized = await RecognizeAsync();

			RecognizeButton.Enabled = true;

			HandleMessage(recognized);
		}

		public void HandleMessage(string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			TextField.Text = text;
		}

		public async Task<string> RecognizeAsync()
		{
			var node = audioEngine.InputNode;
			var recordingFormat = node.GetBusOutputFormat(0);

			var liveSpeechRequest = new SFSpeechAudioBufferRecognitionRequest();
			node.InstallTapOnBus(0, 1024, recordingFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) =>
			{
				// Append buffer to recognition request
				liveSpeechRequest.Append(buffer);
			});

			// Start recording
			audioEngine.Prepare();
			NSError error;
			audioEngine.StartAndReturnError(out error);

			// Did recording start?
			if (error != null)
			{
				return null;
			}

			var taskCompletionSource = new TaskCompletionSource<string>();
			// Start recognition
			var recognitionTask = speechRecognizer.GetRecognitionTask(liveSpeechRequest, (SFSpeechRecognitionResult recognitionResult, NSError err) =>
			{
				// Was there an error?
				if (err != null)
				{
					taskCompletionSource?.SetResult(null);
				}
				else 
				{
					taskCompletionSource?.SetResult(recognitionResult.BestTranscription.FormattedString);
				}
			});

			var result = await taskCompletionSource.Task;
			taskCompletionSource = null;
			audioEngine.Stop();
			liveSpeechRequest.EndAudio();
			node.RemoveTapOnBus(0);
			return result;
		}

		private void HandleVoiceAuthorization(SFSpeechRecognizerAuthorizationStatus status)
		{
			switch (status)
			{
				case SFSpeechRecognizerAuthorizationStatus.Authorized:
					InvokeOnMainThread(() => RecognizeButton.Enabled = true);
					break;

				case SFSpeechRecognizerAuthorizationStatus.Denied:
				case SFSpeechRecognizerAuthorizationStatus.NotDetermined:
				case SFSpeechRecognizerAuthorizationStatus.Restricted:
					TextField.Text = status.ToString();
					break;
			}
		}
	}
}

