using System;
using System.Threading.Tasks;
using Speech;
using UIKit;
using Foundation;
using AVFoundation;

// Hi

namespace VoiceRecognition.Ios
{
	public partial class VoiceRecognitionView : UIViewController
	{
		private AVAudioEngine audioEngine = new AVAudioEngine();
		private SFSpeechRecognizer speechRecognizer = new SFSpeechRecognizer();

		public VoiceRecognitionView() : base("VoiceRecognitionView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			RecognizeButton.TouchUpInside += RecognizeButtonTouchUpInside;
			RecognizeButton.Enabled = false;
			SFSpeechRecognizer.RequestAuthorization(HandleVoiceAuthorization);
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

			if (!string.IsNullOrEmpty(recognized))
			{
				TextField.Text = recognized;
			}

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

