using Bahtinov_Collimator.Sound;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading;

namespace Bahtinov_Collimator.Sound
{
    public class SoundControl
    {
        #region Fields

        private readonly SpeechSynthesizer synthesizer;
        private readonly Queue<string> messageQueue;
        private readonly object lockObject;
        private bool isPlaying;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundControl"/> class.
        /// </summary>
        public SoundControl()
        {
            synthesizer = new SpeechSynthesizer();
            messageQueue = new Queue<string>();
            lockObject = new object();
            isPlaying = false;

            synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;
            synthesizer.SelectVoice("Microsoft Zira Desktop");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a message to the queue and starts playing it if not already in progress.
        /// </summary>
        /// <param name="message">The message to be spoken.</param>
        public void Play(string message)
        {
            lock (lockObject)
            {
                messageQueue.Enqueue(message);

                if (!isPlaying)
                {
                    isPlaying = true;
                    PlayNextMessage();
                }
            }
        }

        /// <summary>
        /// Plays the next message in the queue.
        /// </summary>
        private void PlayNextMessage()
        {
            lock (lockObject)
            {
                if (messageQueue.Count > 0)
                {
                    string message = messageQueue.Dequeue();
                    synthesizer.SpeakAsync(message);
                }
                else
                {
                    isPlaying = false;
                }
            }
        }

        /// <summary>
        /// Stops the current message and clears the queue.
        /// </summary>
        public void StopAndFlush()
        {
            lock (lockObject)
            {
                messageQueue.Clear();

                if (isPlaying)
                {
                    // Set isPlaying to false so that after the current message completes,
                    // the PlayNextMessage method won't play the next message
                    isPlaying = false;
                }
                else
                {
                    // If no message is currently playing, stop the voice immediately
                    StopVoice();
                }
            }
        }

        /// <summary>
        /// Cancels all currently speaking messages.
        /// </summary>
        private void StopVoice()
        {
            synthesizer.SpeakAsyncCancelAll();
            isPlaying = false;
        }

        /// <summary>
        /// Handles the completion of speech synthesis and continues with the next message if available.
        /// </summary>
        private void Synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            Thread.Sleep(10); // Delay between messages (adjust as needed)

            lock (lockObject)
            {
                if (messageQueue.Count > 0)
                {
                    PlayNextMessage();
                }
                else
                {
                    StopVoice();
                }
            }
        }

        #endregion
    }
}





//private void SoundTimer_Tick(object sender, EventArgs e)
//{
//    if (imageCount > lastSoundOut)
//    {
//        Point mousePosition = PointToClient(Control.MousePosition);
//        bool tribatMask = bahtinovLineData.LineAngles.Length > 3;

//        if (RedGroupBoxScreenBounds.Contains(mousePosition))
//        {
//            if (!redSoundPlayed)
//            {
//                soundOut.Play("Red");
//                redSoundPlayed = true;
//                greenSoundPlayed = false; // Reset other flags to allow re-triggering if needed
//                blueSoundPlayed = false;
//            }
//            soundOut.Play(errorValues[0].ToString("F1"));
//        }
//        else if (GreenGroupBoxScreenBounds.Contains(mousePosition) && tribatMask)
//        {
//            if (!greenSoundPlayed)
//            {
//                soundOut.Play("Green");
//                greenSoundPlayed = true;
//                redSoundPlayed = false; // Reset other flags to allow re-triggering if needed
//                blueSoundPlayed = false;
//            }
//            soundOut.Play(errorValues[1].ToString("F1"));
//        }
//        else if (BlueGroupBoxScreenBounds.Contains(mousePosition) && tribatMask)
//        {
//            if (!blueSoundPlayed)
//            {
//                soundOut.Play("Blue");
//                blueSoundPlayed = true;
//                redSoundPlayed = false; // Reset other flags to allow re-triggering if needed
//                greenSoundPlayed = false;
//            }
//            soundOut.Play(errorValues[2].ToString("F1"));
//        }

//        lastSoundOut = imageCount;
//    }
//}


//private Timer soundTimer;
//private int lastSoundOut = 0;
//private SoundControl soundOut;

//private bool redSoundPlayed = false;
//private bool greenSoundPlayed = false;
//private bool blueSoundPlayed = false;


//private void SoundOnOff(bool enable)
//{
//    if (enable)
//    {
//        soundOut.Play("voice enabled");
//        soundTimer.Start();
//    }
//    else
//    {
//        soundOut.StopAndFlush();
//        soundTimer.Stop();
//    }
//}



//private void LoadSettings()
//{
//    if (Properties.Settings.Default.VoiceEnabled)
//    {
//        SoundOnOff(true);
//    }
//    else
//        SoundOnOff(false);
//}


//private void SoundSettings()
//{
//    // sound setup
//    soundOut = new SoundControl();
//    soundTimer = new Timer();
//    soundTimer.Interval = 2000; // 2000 milliseconds
//                                //          soundTimer.Tick += SoundTimer_Tick;
//}
