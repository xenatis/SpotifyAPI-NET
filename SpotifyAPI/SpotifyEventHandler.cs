﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifyAPIv1;

namespace SpotifyAPIv1
{
    public class SpotifyEventHandler
    {
        private Boolean listen = false;
        private System.Timers.Timer timer;
        private SpotifyAPI api;
        private SpotifyMusicHandler mh;

        private StatusResponse response;

        public delegate void TrackChangeEventHandler(TrackChangeEventArgs e);
        public delegate void PlayStateEventHandler(PlayStateEventArgs e);
        public delegate void VolumeChangeEventHandler(VolumeChangeEventArgs e);
        public delegate void TrackTimeChangeEventHandler(TrackTimeChangeEventArgs e);
        public event TrackChangeEventHandler OnTrackChange;
        public event PlayStateEventHandler OnPlayStateChange;
        public event VolumeChangeEventHandler OnVolumeChange;
        public event TrackTimeChangeEventHandler OnTrackTimeChange;

        public SpotifyEventHandler(SpotifyAPI api, SpotifyMusicHandler mh)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 50;
            timer.Elapsed += tick;
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();

            this.api = api;
            this.mh = mh;
        }

        public void ListenForEvents(Boolean listen)
        {
            this.listen = listen;
        }
        public void SetSynchronizingObject(System.ComponentModel.ISynchronizeInvoke obj)
        {
            timer.SynchronizingObject = obj;
        }
        private void tick(object sender, EventArgs e)
        {
            if (!listen)
            {
                timer.Start();
                return;
            }
            api.Update();
            if (response == null)
            {
                response = mh.GetStatusResponse();
                timer.Start();
                return;
            }
            StatusResponse new_response = mh.GetStatusResponse();
            if (!new_response.running && new_response.track == null)
                return;
            if (new_response.track.GetTrackName() != response.track.GetTrackName() && OnTrackChange != null)
            {
                OnTrackChange(new TrackChangeEventArgs()
                {
                    old_track = response.track,
                    new_track = new_response.track
                });
            }
            if (new_response.playing != response.playing && OnPlayStateChange != null)
            {
                OnPlayStateChange(new PlayStateEventArgs()
                {
                    playing = new_response.playing
                });
            }
            if (new_response.volume != response.volume && OnVolumeChange != null)
            {
                OnVolumeChange(new VolumeChangeEventArgs()
                {
                    old_volume = response.volume,
                    new_volume = new_response.volume
                });
            }
            if(new_response.playing_position != response.playing_position && OnTrackTimeChange != null)
            {
                OnTrackTimeChange(new TrackTimeChangeEventArgs()
                {
                    track_time = new_response.playing_position
                });
            }
            response = new_response;
            timer.Start();
        }
    }
}