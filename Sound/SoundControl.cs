using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading;

namespace Bahtinov_Collimator.Sound
{
    public class SoundControl
    {
        private readonly SpeechSynthesizer synthesizer;
        private readonly Queue<string> messageQueue;
        private readonly object lockObject;
        private bool isPlaying;

        public SoundControl()
        {
            synthesizer = new SpeechSynthesizer();
            messageQueue = new Queue<string>();
            lockObject = new object();
            isPlaying = false;

            synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;
            synthesizer.SelectVoice("Microsoft Zira Desktop");
        }

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
                    // If no message is currently playing, we can stop the voice immediately
                    StopVoice();
                }
            }
        }

        private void StopVoice()
        {
            synthesizer.SpeakAsyncCancelAll();
            isPlaying = false;
        }

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
    }
}