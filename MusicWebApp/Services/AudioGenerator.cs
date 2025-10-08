namespace MusicWebApp.Services
{
    public static class AudioGenerator
    {
        private readonly record struct Note(int Midi, double Start, double Duration, float Velocity);

        public static byte[] Generate(ulong seed, int id)
        {
            int intSeed = seed.GetHashCode() ^ id.GetHashCode();
            var rng = new Random(intSeed);

            int bars = 8;
            const int sampleRate = 44100;
            const int beatsPerBar = 4;
            int bpm = 80 + 40 * rng.Next(0, 5);
            double secondsPerBeat = 60.0 / bpm;
            int totalBeats = bars * beatsPerBar;
            double totalSeconds = totalBeats * secondsPerBeat + 0.5;
            int totalSamples = (int)Math.Ceiling(totalSeconds * sampleRate);

            var buffer = new float[totalSamples];

            int rootMidi = rng.Next(48, 60);
            var majorScale = new[] { 0, 2, 4, 5, 7, 9, 11 };
            var progression = new[] { 0, 4, 5, 3 };

            var notes = new List<Note>();
            double beatTime = secondsPerBeat;

            for (int bar = 0; bar < bars; bar++)
            {
                int degree = progression[bar % progression.Length];
                int chordRootMidi = rootMidi + majorScale[degree];
                
                var chord = new[] { chordRootMidi, chordRootMidi + 4, chordRootMidi + 7 };
                foreach (var m in chord)
                {
                    notes.Add(new Note(m, (bar * beatsPerBar) * beatTime, beatsPerBar * beatTime, 0.12f));
                }

                notes.Add(new Note(chordRootMidi - 12, (bar * beatsPerBar) * beatTime, beatTime * 0.95, 0.22f));

                double cur = (bar * beatsPerBar) * beatTime;
                for (int beat = 0; beat < beatsPerBar; beat++)
                {
                    for (int sub = 0; sub < 2; sub++)
                    {
                        int offset = rng.Next(-2, 3);
                        int scaleIndex = Math.Clamp(Array.IndexOf(majorScale, (chordRootMidi - rootMidi) % 12) + offset, 0, majorScale.Length - 1);
                        int melodMidi = rootMidi + majorScale[scaleIndex] + 12;
                        double durBeats = rng.NextDouble() < 0.2 ? 1.0 : 0.5;
                        double dur = durBeats * beatTime * 0.95;
                        notes.Add(new Note(melodMidi, cur, dur, 0.18f));
                        cur += 0.5 * beatTime;
                    }
                }
            }

            foreach (var n in notes)
            {
                SynthesizeNote(n, buffer, sampleRate);
            }

            for (int b = 0; b < totalBeats; b++)
            {
                double t0 = b * beatTime;
                AddKick(t0, buffer, sampleRate, 0.7f);
            }

            float max = buffer.AsSpan().ToArray().Select(MathF.Abs).DefaultIfEmpty(0f).Max();
            if (max > 0.99f)
            {
                float g = 0.99f / max;
                for (int i = 0; i < buffer.Length; i++) buffer[i] *= g;
            }

            return GenerateWav16(buffer, sampleRate);
        }

        private static void SynthesizeNote(Note n, float[] buffer, int sampleRate)
        {
            int start = (int)(n.Start * sampleRate);
            int length = Math.Max(1, (int)(n.Duration * sampleRate));
            int end = Math.Min(buffer.Length, start + length);
            double freq = MidiToFreq(n.Midi);

            double attack = Math.Min(0.02, n.Duration * 0.2);
            double decay = Math.Min(0.1, n.Duration * 0.3);
            double release = Math.Min(0.08, n.Duration * 0.3);
            double sustain = 0.7;

            for (int i = start; i < end; i++)
            {
                double t = (i - start) / (double)sampleRate;
                double env;
                if (t < attack)
                {
                    env = t / Math.Max(attack, 1e-6);
                }
                else if (t < attack + decay)
                {
                    double u = (t - attack) / Math.Max(decay, 1e-6);
                    env = 1.0 + (sustain - 1.0) * u;
                }
                else if (t < n.Duration - release)
                {
                    env = sustain;
                }
                else
                {
                    double u = (t - (n.Duration - release)) / Math.Max(release, 1e-6);
                    env = sustain * (1.0 - Math.Clamp(u, 0, 1));
                }

                double tt = i / (double)sampleRate;
                double sample = SoftSynth(tt, freq) * env * n.Velocity;
                double v = buffer[i] + sample;
                buffer[i] = (float)v;
            }
        }

        private static void AddKick(double startSec, float[] buffer, int sampleRate, float gain)
        {
            int start = (int)(startSec * sampleRate);
            int len = (int)(0.2 * sampleRate);
            for (int i = 0; i < len && (start + i) < buffer.Length; i++)
            {
                double t = i / (double)sampleRate;
                double f = 100.0 * Math.Pow(0.5, t / 0.2);
                double env = Math.Exp(-t * 18);
                double s = Math.Sin(2 * Math.PI * f * t) * env * gain;
                buffer[start + i] += (float)s;
            }
        }

        private static double SoftSynth(double t, double freq)
        {
            double s1 = Math.Sin(2 * Math.PI * freq * t);
            double s2 = 0.3 * Math.Sin(2 * Math.PI * 2 * freq * t);
            double sq = 0.15 * Math.Tanh(Math.Sin(2 * Math.PI * freq * t) * 2.5);
            return (s1 + s2 + sq) * 0.8;
        }

        private static double MidiToFreq(int midi) => 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);

        private static byte[] GenerateWav16(float[] samples, int sampleRate)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int numChannels = 1;
            int bitsPerSample = 16;
            int byteRate = sampleRate * numChannels * bitsPerSample / 8;
            short blockAlign = (short)(numChannels * bitsPerSample / 8);
            int dataSize = samples.Length * numChannels * bitsPerSample / 8;

            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataSize);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)numChannels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write((short)bitsPerSample);

            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(dataSize);

            for (int i = 0; i < samples.Length; i++)
            {
                int v = (int)Math.Round(Math.Clamp(samples[i], -1f, 1f) * short.MaxValue);
                bw.Write((short)v);
            }

            bw.Flush();
            return ms.ToArray();
        }
    }
}
