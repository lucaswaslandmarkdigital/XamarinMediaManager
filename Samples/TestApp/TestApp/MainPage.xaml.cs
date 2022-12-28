using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaManager;
using MediaManager.Library;
using Xamarin.Forms;

namespace TestApp
{
    public partial class MainPage : ContentPage
    {

        public string Track { get; set; }
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            CrossMediaManager.Current.Queue.QueueChanged += Queue_QueueChanged;
        }

        private void Queue_QueueChanged(object sender, MediaManager.Queue.QueueChangedEventArgs e)
        {
            Track = e?.MediaItem?.Title;
            this.OnPropertyChanged(nameof(Track));
        }

        public IList<string> Mp3UrlList => new[]{
            "https://storage.googleapis.com/uamp/The_Kyoto_Connection_-_Wake_Up/01_-_Intro_-_The_Way_Of_Waking_Up_feat_Alan_Watts.mp3",
            "https://ia800605.us.archive.org/32/items/Mp3Playlist_555/CelineDion-IfICould.mp3",
            "https://ia800806.us.archive.org/15/items/Mp3Playlist_555/AaronNeville-CrazyLove.mp3",
            "https://ia800605.us.archive.org/32/items/Mp3Playlist_555/Daughtry-Homeacoustic.mp3",
            
            "https://aphid.fireside.fm/d/1437767933/02d84890-e58d-43eb-ab4c-26bcc8524289/d9b38b7f-5ede-4ca7-a5d6-a18d5605aba1.mp3"
            };


        async void PlayButton_Clicked(System.Object sender, System.EventArgs e)
        {
            await CrossMediaManager.Current.Play(Mp3UrlList);

        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            //await CrossMediaManager.Current.Play(Mp3UrlList);

            CrossMediaManager.Current.ClearQueueOnPlay = false;

            await CrossMediaManager.Current.Stop();
            CrossMediaManager.Current.Queue.Clear();
            foreach (var item in Mp3UrlList)
            {
                var mediaItem = new MediaItem(item);
                CrossMediaManager.Current.Queue.Add(mediaItem);
            }
            await CrossMediaManager.Current.PlayQueueItem(2);
        }

    }
}

