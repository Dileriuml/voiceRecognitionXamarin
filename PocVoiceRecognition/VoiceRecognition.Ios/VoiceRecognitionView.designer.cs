// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CanI.Ios
{
	[Register ("VoiceRecognitionView")]
	partial class VoiceRecognitionView
	{
		[Outlet]
		UIKit.UIButton RecognizeButton { get; set; }

		[Outlet]
		UIKit.UITextField TextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TextField != null) {
				TextField.Dispose ();
				TextField = null;
			}

			if (RecognizeButton != null) {
				RecognizeButton.Dispose ();
				RecognizeButton = null;
			}
		}
	}
}
