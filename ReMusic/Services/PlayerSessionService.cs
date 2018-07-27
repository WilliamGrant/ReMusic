using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReMusic.MusicPlayer.Controller;
using System.Threading;
using Windows.Media;
using ReMusic.MusicPlayer.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using Windows.Media.Playback;

namespace ReMusic.Services
{
    public class PlayerSessionService : ObservableObject
    {
        #region 私有属性
        private static IMusicPlayerController _musicPlayerController;
        private readonly Timer _askPositionTimer;
        private static readonly TimeSpan _askPositionPeriod = TimeSpan.FromSeconds(0.25);
        private bool _autoPlay = false;
        private bool _canPrevious;
        private bool _canPause;
        private bool _canPlay;
        private bool _canNext;
        private bool _isPlay;
        private MediaPlaybackStatus _playbackStatus = MediaPlaybackStatus.Closed;
        private IList<TrackInfo> _playlist = new List<TrackInfo>();
        private TrackInfo _currentTrack;
        private TimeSpan? _duration;
        private TimeSpan _position;
        private bool _isMuted;
        private double _volume;
        private PlayMode _playMode;
        #endregion

        #region 属性访问器集合
        public bool CanPrevious
        {
            get
            {
                return _canPrevious;
            }
            private set
            {
                Set(ref _canPrevious, value);
            }
        }
        public bool CanPlay
        {
            get
            {
                return _canPlay;
            }
            private set
            {
                Set(ref _canPlay, value);
            }

        }
        public bool CanPause
        {
            get { return _canPause; }
            private set
            {
                Set(ref _canPause, value);
            }
        }
        public bool IsPlaying
        {
            get { return _isPlay; }
            private set
            {
                Set(ref _isPlay, value);
            }
        }
        public bool CanNext
        {
            get { return _canNext; }
            private set
            {
                Set(ref _canNext, value);

            }
        }
        public bool IsMuted
        {
            get { return _isMuted; }
            set { Set(ref _isMuted, value); }
        }
        public double Volume
        {
            get { return _volume; }
            set
            {
                if (Set(ref _volume, value))
                    OnVolumeChanged();
            }
        }       
        public TrackInfo CurrentTrack
        {
            get { return _currentTrack; }
            private set
            {
                if (Set(ref _currentTrack, value))
                {
                    _musicPlayerController.SetCurrentTrack(value);
                    OnCurrentTrackChanged();
                }
            }
        }

        public TimeSpan? Duration
        {
            get { return _duration; }
            private set { Set(ref _duration, value); }
        }

        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                var oldValue = _position;
                if (Set(ref _position, value))
                    OnPositionChanged(oldValue, value);
            }
        }
        public PlayMode PlayMode
        {
            get { return _playMode; }
            private set
            {
                if (Set(ref _playMode, value))
                {
                    OnPlayModeChanged();
                }
            }
        }

        public IList<TrackInfo> Playlist
        {
            get { return _playlist; }
            private set
            {
                if (Set(ref _playlist, value))
                {
                    _musicPlayerController.SetPlaylist(_playlist);
                }
            }
        }

        public MediaPlaybackStatus PlaybackStatus
        {
            get { return _playbackStatus; }
            private set
            {
                if (Set(ref _playbackStatus, value))
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(OnCurrentTrackChanged);//检查UI是否被使用,否则释放UI线程资源
                }
            }
        }

        #endregion

        #region 属性变化句柄方法

        private void OnPositionChanged(TimeSpan oldValue, TimeSpan value)
        {
            if (PlaybackStatus != MediaPlaybackStatus.Changing)
            {
                if (Math.Abs(oldValue.Subtract(value).TotalMilliseconds) > 100)//最小调整值为0.1秒
                {
                    SuspendAskPosition();
                    _musicPlayerController.SetPosition(value);
                }
            }
        }

        private void OnPlayModeChanged()
        {
            _musicPlayerController.SetPlayMode(PlayMode);
        }
        private void OnVolumeChanged()
        {
            _musicPlayerController.SetVolume(Math.Min(Math.Max(0.0, Volume / 100), 1.0));
        }

        private void OnCurrentTrackChanged()
        {
            if (CurrentTrack != null)
            {
                var idx = Playlist.IndexOf(CurrentTrack); //获取当前曲目列表索引
                if (idx != -1)
                {
                    CanPrevious = idx > 0 && PlaybackStatus != MediaPlaybackStatus.Changing;
                    CanNext = idx < Playlist.Count - 1 && PlaybackStatus != MediaPlaybackStatus.Changing;
                }
                else
                {
                    CanPrevious = CanNext = false;
                }
            }
            else
            {
                CanPlay = CanPause = CanPrevious = CanNext = false;
            }
        }
        #endregion

        #region 公共方法
        public void PlayWhenOpened()
        {
            _autoPlay = true;
            if (CanPlay)
                Play();
        }

        public void RequestPlayOrPause()
        {
            if (IsPlaying)
            {
                if (CanPause)
                    Pause();
            }
            else
            {
                if (CanPlay)
                    Play();
            }
        }
        public void RequestNext()
        {
            if (CanNext)
                MoveNext();
        }
        public void RequestPrevious()
        {
            if (CanPrevious)
                MovePrevious();
        }

        public void SetPlaylist(IList<TrackInfo> tracks, TrackInfo current)
        {
            Playlist = tracks;
            CurrentTrack = current;
        }
        public void ScrollPlayMode()
        {
            var playmodes = Enum.GetValues(typeof(PlayMode)).Cast<PlayMode>().ToList();
            var nextIdx = playmodes.IndexOf(PlayMode) + 1;
            if (nextIdx >= playmodes.Count)
            {
                nextIdx = 0;
            }
            PlayMode = playmodes[nextIdx];
        }
        #endregion

        #region 私有方法
        private void Play()
        {
            //  PlaybackStatus = MediaPlaybackStatus.Changing;
            _musicPlayerController.Play();
        }
        private void Pause()
        {
            //  PlaybackStatus = MediaPlaybackStatus.Changing;
            _musicPlayerController.Pause();
        }
        private void MoveNext()
        {
            //  PlaybackStatus = MediaPlaybackStatus.Changing;
            _autoPlay = true;
            _musicPlayerController.MoveNext();
        }
        private void MovePrevious()
        {
            //  PlaybackStatus = MediaPlaybackStatus.Changing;
            _autoPlay = true;
            _musicPlayerController.MovePrevious();
        }
        private void OnAskPosition(object state)
        {
            _musicPlayerController.AskPosition();
        }
        private void SuspendAskPosition()
        {
            _askPositionTimer.Change(TimeSpan.Zero, _askPositionPeriod);
        }
        private void ResumeAskPosition()
        {
            _askPositionTimer.Change(TimeSpan.Zero, _askPositionPeriod);
        }
        private void LoadState()
        {
            //var configService = IoC.Get<IConfigurationService>();
            //_playerConfig = configService.Player;
            //PlayMode = _playModeManager.GetProvider(_playerConfig.PlayMode);
            Volume = 50;
            PlayMode = PlayMode.Loop;
            //_playerConfig.EqualizerParameters.CollectionChanged += EqualizerParameters_CollectionChanged;
        }

        #endregion

        #region 音乐播放控制器句柄
        public void NotifyControllerReady()
        {
            OnPlayModeChanged();
            OnVolumeChanged();
        }
        public void NotifyMediaOpened()
        {
            //if (_autoPlay)
            //{
            //    Play();
            //    _autoPlay = false;
            //}
        }
        public async void NotifyControllerStateChanged(MediaPlayerState state)
        {
            await DispatcherHelper.RunAsync(() => 
            {
                switch (state)
                {
                    case MediaPlayerState.Closed:
                        CanPlay = CanPause = false;
                        IsPlaying = false;
                        PlaybackStatus = MediaPlaybackStatus.Closed;
                        SuspendAskPosition();
                        break;
                    case MediaPlayerState.Opening:
                        CanPlay = CanPause = false;
                        IsPlaying = false;
                        PlaybackStatus = MediaPlaybackStatus.Changing;
                        break;
                    case MediaPlayerState.Buffering:
                        CanPlay = CanPause = false;
                        IsPlaying = false;
                        PlaybackStatus = MediaPlaybackStatus.Changing;
                        break;
                    case MediaPlayerState.Playing:
                        CanPause = true;
                        CanPlay = true;
                        IsPlaying = true;
                        PlaybackStatus = MediaPlaybackStatus.Playing;
                        ResumeAskPosition();
                        break;
                    case MediaPlayerState.Paused:
                        CanPause = false;
                        CanPlay = true;
                        IsPlaying = false;
                        PlaybackStatus = MediaPlaybackStatus.Paused;
                        SuspendAskPosition();
                        break;
                    case MediaPlayerState.Stopped:
                        PlaybackStatus = MediaPlaybackStatus.Stopped;
                        SuspendAskPosition();
                        break;
                    default:
                        break;
                }
            });
        }
        public async void NotifyCurrentTrackChanged(TrackInfo track)
        {
            if (_playlist != null)
            {
                var currentTrack = _playlist.FirstOrDefault(o => o == track);
                await DispatcherHelper.RunAsync(() =>
                {
                    if (Set(ref _currentTrack, track))
                    {
                        RaisePropertyChanged(nameof(currentTrack));
                        OnCurrentTrackChanged();
                    }
                    _position = TimeSpan.Zero;
                    RaisePropertyChanged(nameof(Position));
                    Duration = currentTrack.Duration;
                });
            }
        }
        public void NotifyDuration(TimeSpan? duration)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => { Duration = duration; });
        }
        public async void NotifyPosition(TimeSpan position)
        {
            await DispatcherHelper.UIDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (PlaybackStatus == MediaPlaybackStatus.Playing)
                {
                    _position = position;
                    RaisePropertyChanged(nameof(Position));
                }
            });
        }
        public void NotifySeekCompleted()
        {
            ResumeAskPosition();
        }
        public void NotifyPlaylist(IList<TrackInfo> playlist)
        {
            Set(ref _playlist, playlist ?? new List<TrackInfo>(), nameof(Playlist));
        }
        #endregion
        public void Dispose()
        {
            if (_askPositionTimer != null)
            {
                _askPositionTimer.Dispose();
            }
        }
    }
}
