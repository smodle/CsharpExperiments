﻿using System;
using Xamarin.Forms;
using Diary.Shared;
using Xamarin.Auth;

namespace Diary
{
	public partial class EditorPage : ContentPage
	{
		DiaryEntry entry;
		Account account;

		const string kmKey = "keymaterial";

		public EditorPage (Account account, DiaryEntry entry = null)
		{
			this.account = account;
			this.entry = entry == null ? new DiaryEntry () { AccountName = account == null ? "" : account.Username } : entry;

			InitializeComponent ();

			if (this.entry.CipherText != null)
				editorEntry.Text = GetDiaryText (this.entry.CipherText);

			btnSave.Clicked += BtnSaveClicked;
			btnCancel.Clicked += BtnCancelClicked;
		}

		void BtnSaveClicked (object sender, EventArgs e)
		{
			if (editorEntry.Text != null) 
			{
				//encrypt
				entry.CipherText = GetCipherText(editorEntry.Text);

				if (entry.CipherText != null)
					App.Store.SaveEntryAsync (entry);
			}

			this.Navigation.PopAsync ();
		}

		void BtnCancelClicked (object sender, EventArgs e)
		{
			this.Navigation.PopAsync ();
		}

		string GetDiaryText (byte[] cipherText)
		{
			//TODO
			return CryptoUtilities.ByteArrayToString (cipherText);
		}

		byte[] GetCipherText (string diaryText)
		{
			//TODO
			return CryptoUtilities.StringToByteArray (diaryText);
		}
	}
}