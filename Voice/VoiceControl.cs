using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Forms;

using Bahtinov_Collimator;

namespace Bahtinov_Collimator.Voice
{
    /// <summary>
    /// Provides control over voice synthesis, including queuing and playing voice messages.
    /// </summary>
    public class VoiceControl
    {
        #region Fields

        private readonly SpeechSynthesizer synthesizer;
        private readonly Queue<(string Text, int Rate)> messageQueue;
        private readonly Dictionary<int, double> errorValues;
        private bool voiceEnabled;
        private int channelPlaying = -1;
        private int newSpeechRate = 0;
        private readonly Dictionary<int, double> lastPlayedValues = new Dictionary<int, double>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceControl"/> class.
        /// Sets up the speech synthesizer, message queue, and event handlers.
        /// </summary>
        public VoiceControl()
        {
            errorValues = new Dictionary<int, double>();
            synthesizer = new SpeechSynthesizer();
            messageQueue = new Queue<(string, int)>();
            synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;
            ApplyVoiceForCurrentLanguage();

            BahtinovProcessing.FocusDataEvent += FocusDataEvent;
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;

            LoadSettings();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Selects an installed speech voice that matches <see cref="LanguageLoader.CurrentLanguageCode"/> (e.g. German for the de pack).
        /// </summary>
        private void ApplyVoiceForCurrentLanguage()
        {
            string code = LanguageLoader.CurrentLanguageCode ?? LanguageLoader.DefaultLanguageCode;
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(code);
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.GetCultureInfo(LanguageLoader.DefaultLanguageCode);
            }

            try
            {
                synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, culture);
                return;
            }
            catch (InvalidOperationException)
            {
            }

            string twoLetter = culture.TwoLetterISOLanguageName;
            foreach (InstalledVoice installed in synthesizer.GetInstalledVoices())
            {
                if (!installed.Enabled)
                    continue;
                if (string.Equals(installed.VoiceInfo.Culture.TwoLetterISOLanguageName, twoLetter, StringComparison.OrdinalIgnoreCase))
                {
                    synthesizer.SelectVoice(installed.VoiceInfo.Name);
                    return;
                }
            }

            try
            {
                synthesizer.SelectVoice("Microsoft Zira Desktop");
            }
            catch (InvalidOperationException)
            {
                InstalledVoice first = synthesizer.GetInstalledVoices().FirstOrDefault(v => v.Enabled);
                if (first != null)
                    synthesizer.SelectVoice(first.VoiceInfo.Name);
            }
        }

        /// <summary>
        /// Handles the focus data event by updating the error values for the specified group. 
        /// If the value for the currently playing channel has changed since the last call, 
        /// the new value is played and announced. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="FocusDataEventArgs"/> object that contains the event data, 
        /// including the focus value (Bahtinov offset) and group identifier.</param>
        public void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            double focusValue = e.FocusData.BahtinovOffset;
            int group = e.FocusData.Id;

            if (group == -1)
            {
                errorValues.Clear();
                lastPlayedValues.Clear();
            }
            else
            {
                errorValues[group] = focusValue;

                if (channelPlaying != -1 && errorValues.ContainsKey(channelPlaying))
                {
                    double currentValue = errorValues[channelPlaying];

                    if (!lastPlayedValues.ContainsKey(channelPlaying) || lastPlayedValues[channelPlaying] != currentValue)
                    {
                        Play(FormatFocusValueForSpeech(currentValue), 2);
                        Console.WriteLine(currentValue.ToString("F1"));

                        lastPlayedValues[channelPlaying] = currentValue;
                    }
                }
            }
        }

        /// <summary>
        /// Handles channel selection events to update the current channel and play the associated error value.
        /// </summary>
        private void ChannelSelected(object sender, ChannelSelectEventArgs e)
        {
            channelPlaying = -1;
            Stop();

            for (int i = 0; i < e.ChannelSelected.Length; i++)
            {
                if (e.ChannelSelected[i] && i < errorValues.Count)
                {
                    channelPlaying = i;
                    Play(string.Format(GetUiCulture(), UiText.Current.VoiceChannelAnnouncementFormat ?? "Channel {0}", i + 1), 2);
                    Play(FormatFocusValueForSpeech(errorValues[i]), 2);
                    break;
                }
            }
        }

        /// <summary>
        /// Formats a focus offset for speech using the UI language’s decimal separator and drops a redundant leading zero (e.g. 0.3 → .3).
        /// </summary>
        private string FormatFocusValueForSpeech(double value)
        {
            CultureInfo culture = GetUiCulture();
            string dec = culture.NumberFormat.NumberDecimalSeparator;
            string result = value.ToString("F1", culture);

            if (value > -1 && value < 1)
            {
                string zeroPrefix = "0" + dec;
                string negZeroPrefix = "-0" + dec;
                if (result.StartsWith(zeroPrefix, StringComparison.Ordinal))
                    result = result.Substring(1);
                else if (result.StartsWith(negZeroPrefix, StringComparison.Ordinal))
                    result = "-" + result.Substring(2);
            }

            return result;
        }

        private static CultureInfo GetUiCulture()
        {
            string code = LanguageLoader.CurrentLanguageCode ?? LanguageLoader.DefaultLanguageCode;
            try
            {
                return CultureInfo.GetCultureInfo(code);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.GetCultureInfo(LanguageLoader.DefaultLanguageCode);
            }
        }

        /// <summary>
        /// Loads settings and updates the voice enabled state based on configuration.
        /// </summary>
        public void LoadSettings()
        {
            voiceEnabled = Properties.Settings.Default.VoiceEnabled;

            if (voiceEnabled)
                Play(UiText.Current.VoiceEnabledAnnouncement ?? "Voice enabled", 0);
            else
                Play(UiText.Current.VoiceDisabledAnnouncement ?? "Voice disabled", 0);
        }

        /// <summary>
        /// Queues a message for playback if voice synthesis is enabled.
        /// </summary>
        /// <param name="text">The text to be spoken.</param>
        /// <param name="speechRate">The speech rate for the message.</param>
        public void Play(string text, int speechRate)
        {
            if (voiceEnabled)
            {
                if (synthesizer.State == SynthesizerState.Ready || synthesizer.State == SynthesizerState.Speaking)
                {
                    if (messageQueue.Count == 0)
                    {
                        messageQueue.Enqueue((text, speechRate));
                    }

                    if (synthesizer.State == SynthesizerState.Ready)
                    {
                        PlayNextMessage();
                    }
                }
            }
        }

        /// <summary>
        /// Plays the next message in the queue, if available.
        /// </summary>
        private void PlayNextMessage()
        {
            if (messageQueue.Count > 0)
            {
                var (nextMessage, rate) = messageQueue.Dequeue();
                newSpeechRate = rate;
                synthesizer.SpeakCompleted += OnSpeakCompleted;
                synthesizer.SpeakAsync(nextMessage);
            }
        }

        /// <summary>
        /// Handles the completion of speech synthesis to update the speech rate.
        /// </summary>
        private void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            synthesizer.SpeakCompleted -= OnSpeakCompleted;
            synthesizer.Rate = newSpeechRate;
        }

        /// <summary>
        /// Handles the completion of speech synthesis to play the next message in the queue.
        /// </summary>
        private void Synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            PlayNextMessage();
        }

        /// <summary>
        /// Stops all ongoing speech and clears the message queue.
        /// </summary>
        public void Stop()
        {
            synthesizer.SpeakAsyncCancelAll();
            messageQueue.Clear();
        }

        #endregion
    }
}
