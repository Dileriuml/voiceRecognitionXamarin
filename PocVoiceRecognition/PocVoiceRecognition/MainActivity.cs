using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Android.Content;
using Java.Util;
using PocVoiceRecognition.CanI.Droid.Common;

namespace PocVoiceRecognition
{
	[Activity(Label = "Can I", MainLauncher = true, Icon = "@mipmap/icon")]
	[IntentFilter(new[] { Intent.ActionSearch, Intent.ActionView, CommonIntents.VoiceSearchAction },
		Categories = new[] { Intent.CategoryDefault })]
	public class MainActivity : Activity
	{
		private const int VoiceCode = 100;
		private EditText textEdit;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			textEdit = FindViewById<EditText>(Resource.Id.editText);
			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.myButton);

			button.Click += ButtonClick;

			if (Intent.Action == Intent.ActionSearch || Intent.Action == CommonIntents.VoiceSearchAction)
			{
				var queryContent = Intent.GetStringExtra(SearchManager.Query);
				// You can perform search from here
				textEdit.Text = queryContent;
			}
		}

		private void ButtonClick(object sender, System.EventArgs e)
		{
			var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
			voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
			voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "Can I ...");
			voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
			voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
			voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
			voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
			voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Locale.Default);

			try
			{
				StartActivityForResult(voiceIntent, VoiceCode);
			}
			catch (ActivityNotFoundException a)
			{
				Toast.MakeText(ApplicationContext, "No activity to recognize speech", ToastLength.Short).Show();
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == VoiceCode)
			{
				if (resultCode == Result.Ok)
				{
					var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
					if (matches.Count != 0)
					{
						string textInput = matches[0];
						textEdit.Text = textInput;
					}
				}
				else
				{
					textEdit.Text = "No text recognized";
				}
			}

			base.OnActivityResult(requestCode, resultCode, data);
		}
	}
}

