using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ochawan
{
    public partial class GameForm : Form
    {
        Graphics grfx;
        MMLParser mml;
        Sound sound = new Sound();
        bool isRecording = false;

        public GameForm()
        {
            InitializeComponent();

            var bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = bmp;
            grfx = Graphics.FromImage(pictureBox1.Image);
            mml = new MMLParser(sound);
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            clear();
            pictureBox1.Refresh();
        }

        void clear()
        {
            grfx.Clear(Color.WhiteSmoke);
            grfx.FillRectangle(Brushes.Black, 0, 0, 512, 1);
            grfx.FillRectangle(Brushes.Red, 0, 256, 512, 1);
            grfx.FillRectangle(Brushes.Black, 0, 512, 512, 1);
            Debug.WriteLine(this.BackColor);
        }

        public void Play(string chat)
        {
            mml.Parse(chat);
        }

        public void putStone(int xpos, int ypos)
        {
            grfx.FillRectangle(Brushes.GreenYellow, xpos, ypos, 4, 4);
            pictureBox1.Refresh();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            
            
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            e.SuppressKeyPress = true;

            mml.Parse(textBox1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRecording) return;
            isRecording = true;
            sound.Record(2);
            isRecording = false;

            trackBar1.Minimum = 0;
            trackBar1.Maximum = sound.CaptureMaxCount - 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clear();
            pictureBox1.Refresh();

            if (sound.capture == null)
                return;
            var data = sound.capture[trackBar1.Value];
            //var data = Osc.Sine((1 << 14) / 2, 440).array;
            sound.Add(data);
            sound.WriteBuffer();
            if (trackBar1.Value < trackBar1.Maximum)
                trackBar1.Value++;
            else
                trackBar1.Value = 0;
            label1.Text = trackBar1.Value.ToString();

            var result = FFT.Transform(data);
            for (int i = 0; i < 512; i++)
            {
                drawLine(Pens.Green, i, (int)(data[i * 4] * 512), (int)(data[(i + 1) * 4] * 512));
                drawLine(Pens.Blue, i, (int)(result[i * 8]), (int)(result[(i + 1) * 8]));
            }
            pictureBox1.Refresh();
            
            int idx = 0;
            int maxLength = result.Length / 2;
            double max = 0;
            for (int i = 10; i < maxLength; i++)
            {
                var val = Math.Abs(result[i]);
                if (val > max)
                {
                    max = val;
                    idx = i;
                }
            }
            var freq = idx * sound.FreqResolution / 2;
            Func<double, double> ToNote = (f) => { return 69 + 12 * Math.Log(f / 440, 2); };
            label2.Text = freq.ToString("f") + "Hz±" + sound.FreqResolution.ToString("f");
            label3.Text = max.ToString("f") + ": " + ToNote(freq - sound.FreqResolution).ToString("f")
                + "-" + ToNote(freq + sound.FreqResolution).ToString("f");
            label4.Text = data[idx].ToString("f4");
            var note = (int)ToNote(freq);
            if (note < 0) return;
            textBox1.Text += "v" + (int)(1000 * Math.Abs(data[idx])) + "o" + note / 12 + MMLParser.NoteNumToAlp(note % 12);
            
        }

        void drawLine(Pen pen, int x, int y1, int y2)
        {
            grfx.DrawLine(pen, x, y1 + 256, x + 1, y2 + 256);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            mml.Parse(textBox1.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void writingBufferTimer_Tick(object sender, EventArgs e)
        {
            sound.WriteBuffer();
        }
    }
    
    enum Sulr { Before, Current, After, None };

    class MMLParser
    {
        Sound sound;
        Regex pattern = new Regex(
            @"@[vwndp]?-?\d{1,3}|\[(?:[a-g][\+\-]?\d{0,3}\.?)+\]|[a-g][\+\-]?\d{0,3}\.?|[rlotvq]\d{0,3}|[<>\(\)]|@e1(,\s*\d{1,3}\s*){4,}|&");
        Regex semitonePattern = new Regex(@"[\+\-]");
        Regex macroPattern    = new Regex(@"\$\w+");
        Regex macroDefPattern = new Regex(@"(\$\w+)\s*=((?:\w|\d|\s|[\+\-<>\(\)@,])+);");
        Dictionary<string, string> macros = new Dictionary<string, string>();
        Melody[] melodies;
        const double ampMax = 0.7;
        const int oneMinute = 60;
        const int oneBar = 4;
        double numSample;
        int gatetime = 15;
        double amplitude = ampMax * 12.0 / 16.0;
        int defaultLength = 4;
        int notelength = 4;
        int tempo = 120;
        int octave = 4;
        int sampleRate;
        int channels;
        int waveform = 6;

        public MMLParser(Sound snd)
        {
            sound = snd;
            sampleRate = sound.SampleRate;
            channels = sound.Channels;
            numSample = sampleRate * oneMinute * oneBar / tempo;
        }

        public void Parse(string input)
        {
            var expand = expandLoop(macro(input));

            var tracks = expand.ToLower().Split(';');
            melodies = new Melody[tracks.Count()];

            notelength = 4;
            defaultLength = 4;

            tempo = 120;
            numSample = sampleRate * oneMinute * oneBar / tempo;

            for (int i = 0; i < tracks.Count(); i++)
            {
                initialize();
                melodies[i] = new Melody();
                var notes = pattern.Matches(tracks[i]);
                var memo = parseAmpersand(notes);
                for (int n = 0; n < notes.Count; n++)
                {
                    Debug.WriteLine(notes[n].Value);
                    parseNote(melodies[i], notes[n].Value, memo[n]);
                }
            }
            
            sound.Add(mix(melodies));
        }

        string macro(string str)
        {
            var defs = macroDefPattern.Matches(str);
            foreach (Match def in defs)
            {
                Debug.WriteLine(def);
                str = str.Replace(def.Value, "");
                var name = def.Groups[1].Value;
                Debug.WriteLine("name: " + name);
                var content = def.Groups[2].Value;
                Debug.WriteLine("content: " + content);
                if (macros.ContainsKey(name))
                    macros.Remove(name);
                macros.Add(name, content);
            }
            var replacements = macroPattern.Matches(str);
            var res = new HashSet<string>();
            foreach (Match re in replacements)
            {
                if (macros.ContainsKey(re.Value))
                    res.Add(re.Value);
            }
            foreach (string re in res)
            {
                str = str.Replace(re, macros[re]);
            }
            return str;
        }

        string expandLoop(string input)
        {
            var str = removeComment(input);
            var expands = new Stack<StringBuilder>();
            var rewind = false;
            expands.Push(new StringBuilder());
            var head = expands.First();
            var rewinds = new List<int>();
            var repeats = new Stack<int>();
            for (int i = 0; i < str.Length; i++)
                if (str[i] == '/' && str[i + 1] == ':')
                {
                    int repeat;
                    if (int.TryParse(str[i + 2].ToString(), out repeat))
                    {
                        repeats.Push(repeat);
                        i += 2;
                    }
                    else
                    {
                        repeats.Push(2);
                        i += 1;
                    }
                    expands.Push(new StringBuilder());
                    head = expands.First();
                }
                else if (str[i] == ':' && str[i + 1] == '/' && repeats.Count > 0)
                {
                    i += 1;
                    var inner = expands.Pop();
                    var expand = String.Concat(Enumerable.Repeat(inner, repeats.Pop()));
                    head = expands.First();
                    head.Append(expand);
                    if (rewind)
                    {
                        var curRewind = rewinds.Last();
                        rewinds.RemoveAt(rewinds.Count - 1);
                        head.Remove(head.Length - curRewind - 1, curRewind);
                        if (rewinds.Count == 0)
                            rewind = false;
                        else
                            rewinds[rewinds.Count - 1] += expand.Length - curRewind;
                    }
                }
                else if (str[i] == '/' && expands.Count > 1)
                {
                    rewinds.Add(0);
                    rewind = true;
                }
                else
                {
                    head.Append(str[i]);
                    if (rewind) rewinds[rewinds.Count - 1] += 1;
                }

            return head.ToString();
        }

        string removeComment(string input)
        {
            var ret = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (i < input.Length - 4 && input[i] == '/' && input[i + 1] == '*')
                    while (input[i - 1] == '*' && input[i] == '/')
                        i++;
                else
                    ret.Append(input[i]);
            }
            return ret.ToString();
        }

        void initialize()
        {
            waveform = 6;
            octave = 4;
            gatetime = 15;
            amplitude = ampMax * 12.0 / 16.0;
            Osc.SetEnvelope(new double[4] { 1 / 127.0, 0, 1, 5 / 127.0 });
            Osc.squareDuty = 50;
            Osc.noiseFrequency = 0;
            Osc.detune = 0;
            Osc.pan = 64;
        }

        Sulr[] parseAmpersand(MatchCollection notes)
        {
            if (notes.Count == 0)
                return new Sulr[0];
            var ret = new Sulr[notes.Count];
            var max =  notes.Count - 1;
            ret[0] = Sulr.None;
            ret[max] = Sulr.None;
            for (int n = 1; n < max; n++)
            {
                if (notes[n].Value == "&")
                {
                    if (ret[n - 1] == Sulr.None)
                        ret[n - 1] = Sulr.Before;
                    else if (ret[n - 1] == Sulr.After)
                        ret[n - 1] = Sulr.Current;
                    else
                        throw new Exception("parseAmpersand error");
                    ret[n + 0] = Sulr.Current;
                    ret[n + 1] = Sulr.After;
                }
                else if (ret[n] != Sulr.After)
                    ret[n] = Sulr.None;
            }
            return ret;
        }

        void parseNote(Melody melody, string note, Sulr sulr)
        {
            var argnum = note.Substring(1);
            switch (note[0])
            {
                case '&':
                    break;
                case '@':
                    parseAtmark(argnum);
                    break;
                case '<':
                    if (octave < 8)
                        octave++;
                    break;
                case '>':
                    if (octave > 0)
                        octave--;
                    break;
                case '(':
                    if (amplitude < ampMax)
                        amplitude += ampMax * 1 / 16.0;
                    break;
                case ')':
                    var ampunit = ampMax * 1 / 16.0;
                    if (amplitude > ampunit)
                        amplitude -= ampunit;
                    break;
                case 'q':
                    gatetime = 15;
                    if (argnum != "")
                    {
                        gatetime = Convert.ToInt32(argnum);
                        gatetime = Math.Min(gatetime, 16);
                        gatetime = Math.Max(gatetime, 1);
                    }
                    break;
                case 'v':
                    var amptemp = 1.0;
                    if (argnum != "")
                        amptemp = ampMax * (Convert.ToInt32(argnum) + 1) / 16.0;
                    amplitude = Math.Min(amptemp, ampMax);
                    break;
                case 't':
                    tempo = 120;
                    if (argnum != "")
                        tempo = Convert.ToInt32(argnum);
                    numSample = sampleRate * oneMinute * oneBar / tempo;
                    break;
                case 'o':
                    octave = 4;
                    if (argnum != "")
                        octave = Convert.ToInt32(argnum);
                    if (octave < 0 || octave > 8)
                        octave = 4;
                    break;
                case 'l':
                    defaultLength = 4;
                    if (argnum != "")
                        defaultLength = Convert.ToInt32(argnum);

                    notelength = defaultLength;
                    break;
                case 'r':
                    notelength = defaultLength;
                    if (argnum != "")
                        notelength = Convert.ToInt32(argnum);
                    var num = Convert.ToInt32(numSample / notelength);
                    melody.Add(Osc.Rest(num));
                    break;
                default:
                    rendering(melody, note[0], argnum, sulr);
                    break;
            }
        }

        void parseAtmark(string argnum)
        {
            var argn = argnum.Substring(1);
            switch (argnum[0])
            {
                case 'v':
                    var amptemp = 1.0;
                    if (argn != "")
                        amptemp = ampMax * (Convert.ToInt32(argn) + 1) / 128.0;
                    amplitude = Math.Min(amptemp, ampMax);
                    break;
                case 'w':
                    Osc.squareDuty = Convert.ToInt32(argn);
                    break;
                case 'n':
                    Osc.noiseFrequency = Convert.ToInt32(argn);
                    break;
                case 'd':
                    Osc.detune = Convert.ToInt32(argn);
                    break;
                case 'p':
                    Osc.pan = Convert.ToInt32(argn);
                    break;
                case 'e':
                    if (!argnum.StartsWith("e1,"))
                        break;
                    argnum = argnum.Substring(3);
                    
                    var unit = 127.0;
                    var arg = argnum.Split(',')
                                    .Select((x) => { return int.Parse(x.Trim()) / unit; });
                    if (arg.Count() % 2 != 0)
                        break;

                    Osc.SetEnvelope(arg.ToArray());
                    break;
                default:
                    waveform = Convert.ToInt32(argnum);
                    switch (waveform)
                    {
                        case 3: Osc.squareDuty = 50; break;
                        case 5: Osc.squareDuty = 1; break;
                    }
                    break;
            }
        }

        Note generate(char note, string argnum, Sulr sulr)
        {
            var notenum = alpToNoteNum(note);
            var semimc = semitonePattern.Match(argnum);
            if (semimc.Success)
            {
                switch (semimc.Value)
                {
                    case "+":
                        notenum++;
                        break;
                    case "-":
                        notenum--;
                        break;
                }
                argnum = argnum.Substring(1);
            }
            var dotted = 1.0;
            if (argnum.EndsWith("."))
            {
                argnum = argnum.Replace(".", "");
                dotted = 1.5;
            }
            notelength = defaultLength;
            if (argnum != "")
                notelength = Convert.ToInt32(argnum);
            var length = numSample / notelength * dotted;

            return Osc.FromIndex(waveform, amplitude, length, notenum);
        }

        Note generateChord(string chord, Sulr sulr)
        {
            var reg = new Regex(@"[a-g][\+\-]?\d{0,3}\.?");
            var mc = reg.Matches(chord);

            var notes = new Melody[mc.Count];

            for (int i = 0; i < mc.Count; i++)
            {
                var m = mc[i].Value;
                notes[i] = new Melody();
                notes[i].Add(generate(m[0], m.Substring(1), sulr));
            }

            return new Note(mix(notes));
        }

        void rendering(Melody melody, char note, string argnum, Sulr sulr)
        {
            Note wave;
            if (note == '[')
            {
                var chord = argnum.Substring(0, argnum.Length - 1);
                wave = generateChord(chord, sulr);
            }
            else
                wave = generate(note, argnum, sulr);

            melody.Add(Osc.Pan(Osc.Envelope(wave, gatetime, sulr)));
            //var gauss = Osc.Gauss(num, 4);
            //sound.Add(ampNote * mainNote * gauss);
        }

        int alpToNoteNum(char alp)
        {
            int note;
            switch (alp)
            {
                case 'c': note = 0;  break;
                case 'd': note = 2;  break;
                case 'e': note = 4;  break;
                case 'f': note = 5;  break;
                case 'g': note = 7;  break;
                case 'a': note = 9;  break;
                case 'b': note = 11; break;
                default: throw new Exception("alpToNoteNum error");
            }
            return note - 69 + octave * 12;
        }

        public static string NoteNumToAlp(int num)
        {
            switch (num)
            {
                case 0: return "c"; 
                case 1: return "c+";
                case 2: return "d"; 
                case 3: return "d+";
                case 4: return "e"; 
                case 5: return "f"; 
                case 6: return "f+";
                case 7: return "g"; 
                case 8: return "g+";
                case 9: return "a";
                case 10: return "a+";
                case 11: return "b";
                default: throw new Exception("alpToNoteNum error");
            }
        }

        double[] mix(Melody[] melodies)
        {
            var count = melodies.Select((m) => { return m.Count(); }).Max();
            var music = new double[count];

            foreach (var m in melodies)
            {
                for (int i = 0, max = m.Count(); i < max; i++)
                    music[i] = (music[i] + m[i]);
            }

            var n = melodies.Length;
            for (int i = 0, max = music.Count(); i < max; i++)
                music[i] = music[i] / n;

            return music;
        }
    }

    class Melody
    {
        private List<double> values = new List<double>();

        public void Add(Note note)
        {
            this.values.AddRange(note.array);
        }

        public int Count()
        {
            return values.Count();
        }

        public double[] ToArray()
        {
            return values.ToArray();
        }

        public double this[int i]
        {
            get { return values[i]; }
            set { this.values[i] = value; }
        }
    }

    class Note
    {
        private double[] values;
        public int count { get { return this.values.Count(); } }
        public double[] array { get { return this.values; } }

        public Note()
        {
            values = new double[0];
        }

        public Note(double[] samples)
        {
            values = samples;
        }

        public static Note operator *(Note lh, Note rh)
        {
            if (lh.count == rh.count)
            {
                var ret = new double[lh.count];
                for (int i = 0; i < lh.count; i++)
                    ret[i] = lh[i] * rh[i];

                return new Note(ret);
            }
            throw new Exception("product: array size is miss match");
        }

        public static Note operator +(Note lh, Note rh)
        {
            if (lh.count == rh.count)
            {
                var ret = new double[lh.count];
                for (int i = 0; i < lh.count; i++)
                    ret[i] = lh[i] + rh[i];

                return new Note(ret);
            }
            throw new Exception("add: array size is miss match");
        }

        public static Note operator &(Note lh, Note rh)
        {
            var list = new List<double>();
            list.AddRange(lh.values);
            list.AddRange(rh.values);

            return new Note(list.ToArray());
        }

        public double this[int i]
        {
            get { return values[i]; }
            set { this.values[i] = value; }
        }
    }

    class Osc
    {
        public static int squareDuty = 50;
        public static int noiseFrequency = 0;
        public static double detune = 0;
        public static int pan = 64;
        private const int sampleRate = 48000;
        private static double attack = sampleRate / 127.0;
        private static double[] decay   = new double[1] { 0 };
        private static double[] sustain = new double[1] { 1 };
        private static double release = sampleRate / 127.0 * 5;

        public static void SetEnvelope(double[] arg)
        {
            attack = arg.First() * sampleRate;
            release = arg.Last() * sampleRate;
            var numPoint = (arg.Count() - 2) / 2;
            decay = new double[numPoint];
            sustain = new double[numPoint];
            for (int i = 0; i < numPoint; i++)
            {
                decay[i] = arg[2 * i + 1] * sampleRate;
                sustain[i] = Math.Min(1, arg[2 * i + 2]);
            }
        }

        public static Note Envelope(Note note, int gatetime, Sulr sulr)
        {
            int idx = 0;
            int max = note.count * gatetime / 16;
            double attackStep = 1 / attack;
            double attackEnv = 0;
            if (sulr == Sulr.After || sulr == Sulr.Current)
            {
                ;
            }
            else
            {
                for (; idx < attack && idx < max; idx++)
                {
                    note[idx] *= attackEnv;
                    attackEnv += attackStep;
                }
                int n;
                double decayStep;
                double decayEnv = 1;
                for (n = 0; n < decay.Count(); n++)
                {
                    decayStep = (1 - sustain[n]) / decay[n];
                    for (; idx < decay[n] && idx < max; idx++)
                    {
                        note[idx] *= decayEnv;
                        decayEnv -= decayStep;
                    }
                }
            }

            if (sulr == Sulr.Before || sulr == Sulr.Current)
            {
                for (int ct = note.count; idx < ct; idx++)
                    note[idx] *= sustain.Last();
            }
            else
            {
                int sustaintime = max - (int)release;
                for (; idx < sustaintime; idx++)
                    note[idx] *= sustain.Last();
                double releaseStep = sustain.Last() / release;
                double releaseEnv = sustain.Last();
                for (; idx < max; idx++)
                {
                    note[idx] *= releaseEnv;
                    releaseEnv -= releaseStep;
                }
                for (int ct = note.count; idx < ct; idx++)
                    note[idx] = 0;
            }
            return note;
        }

        public static Note Pan(Note note)
        {
            pan = Math.Min(pan, 127);
            pan = Math.Max(pan, 1);
            for (int i = 0, max = note.count / 2; i < max; i++)
            {
                note[2 * i + 1] = note[2 * i] / 128.0 * (128 - pan);
                note[2 * i + 0] = note[2 * i] / 128.0 * pan;
            }
            return note;
        }

        public static Note FromIndex(int index, double amplitude, double length, int notenum)
        {
            var freq = Math.Pow(2, (notenum + detune / 100.0) / 12.0) * 440;
            length -= length % (sampleRate / freq);
            var num = (int)length;
            var amp = Amp(num, amplitude);
            switch (index)
            {
                case 0: return amp * Sine(num, freq);
                case 1: return amp * Saw(num, freq);
                case 2: return amp * Triangle(num, freq);
                case 3: return amp * Square(num, freq);
                case 4: return amp * Noise(num);
                case 5: return amp * FCSquare(num, freq);
                case 6: return amp * FCTriangle(num, freq);
                case 7: return amp * FCNoise(num);
                default: return amp * Noise(num);
            }
        }
        public static Note Amp(int num, double amp)
        {
            var samples = new double[num * 2];
            for (int i = 0; i < num; i++)
            {
                samples[2 * i + 0] = amp;
                samples[2 * i + 1] = amp;
            }
            return new Note(samples);
        }

        public static Note Rest(int num)
        {
            var samples = new double[num * 2];
            return new Note(samples);
        }

        public static Note Gauss(int num, int vari)
        {
            var samples = new double[num * 2];
            double smp;
            for (int i = 0; i < num; i++)
            {
                // Oi = (num - 1) / 4
                smp = Math.Exp(-2 * vari / Math.Pow(num - 1, 2) * Math.Pow(i - (num - 1) / 4.0, 2));
                samples[2 * i + 0] = smp;
                samples[2 * i + 1] = smp;
            }
            return new Note(samples);
        }

        public static Note Sine(int num, double freq)
        {
            var samples = new double[num * 2];
            double angle = (Math.PI * 2 * freq) / sampleRate;
            var amp = 1.8;

            for (int i = 0; i < num; i++)
            {
                samples[2 * i + 0] = Math.Sin(angle * i) * amp;
                samples[2 * i + 1] = samples[2 * i];
            }

            return new Note(samples);
        }

        public static Note Saw(int num, double freq)
        {
            var samples = new double[num * 2];
            double period = sampleRate / freq;
            double step = 1 / period;
            int total = 0;
            double sample;
            while (total < num)
            {
                sample = 1;
                for (int i = 0; i < period && total < num; i++)
                {
                    sample -= step;
                    samples[2 * total + 0] = sample;
                    samples[2 * total + 1] = sample;
                    total++;
                }
            }
            return new Note(samples);
        }

        public static Note Triangle(int num, double freq)
        {
            var samples = new double[num * 2];
            double period = sampleRate / freq;
            double half = period / 2;
            double step = 4 / period;
            double sample;
            int total = 0;
            int sign = 1;

            while (total < num)
            {
                sample = -1;
                for (int i = 0; i < period && total < num; i++)
                {
                    if (i < half)
                        sign = 1;
                    else
                        sign = -1;

                    sample += sign * step;
                    samples[2 * total + 0] = sample;
                    samples[2 * total + 1] = sample;
                    total++;
                }
            }
            return new Note(samples);
        }

        public static Note Square(int num, double freq)
        {
            return Square(num, freq, squareDuty);
        }
        public static Note FCSquare(int num, double freq)
        {
            return Square(num, freq, squareDuty * 12.5);
        }
        public static Note Square(int num, double freq, double duty)
        {
            var samples = new double[num * 2];
            double period = sampleRate / freq;
            double ontime = period * duty / 100.0;
            double val;
            int total = 0;

            while (total < num)
            {
                val = 0.5;
                for (int i = 0; i < period && total < num; i++)
                {
                    if (i > ontime)
                        val = -0.5;

                    samples[2 * total + 0] = val;
                    samples[2 * total + 1] = val;
                    total++;
                }
            }
            return new Note(samples);
        }

        public static Note Noise(int num)
        {
            var samples = new double[num * 2];
            var rnd = new Random();
            double sample = 0;
            samples[0] = 0;
            samples[1] = 0;
            double point = rnd.NextDouble() - 0.5;
            for (int i = 1; i < num; i++)
            {
                if (noiseFrequency == 0)
                    sample = rnd.NextDouble() - 0.5;
                else
                {
                    if (i % noiseFrequency == 0)
                        sample = rnd.NextDouble() - 0.5;
                    //sample += (point - sample) / noiseFrequency; 
                }

                samples[2 * i + 0] = sample;
                samples[2 * i + 1] = sample;
            }

            return new Note(samples);
        }

        public static Note FCTriangle(int num, double freq)
        {
            var samples = new double[num * 2];
            double unit = 8.0;
            double period = sampleRate / freq;
            double half = period / 2;
            double step = 4 / period * 8;
            double sample;
            int total = 0;
            int sign = 1;

            while (total < num)
            {
                sample = -8;
                for (int i = 0; i < period && total < num; i++)
                {
                    if (i < half)
                        sign = 1;
                    else
                        sign = -1;

                    sample += sign * step;
                    samples[2 * total + 0] = Math.Round(sample) / unit;
                    samples[2 * total + 1] = samples[2 * total];
                    total++;
                }
            }
            return new Note(samples);
        }

        public static Note FCNoise(int num)
        {
            var samples = new double[num * 2];
            int reg = 0x8000;

            for (int i = 0; i < num; i++)
            {
                reg >>= 1;
                reg |= ((reg ^ (reg >> 1)) & 1) << 15;

                samples[2 * i + 0] = (double)(reg & 1);
                samples[2 * i + 1] = samples[2 * i];
            }
            return new Note(samples);
        }
    }

    class FFT
    {
        static public double[] Transform(float[] data)
        {
            var length = data.Length;
            var ret = new double[length];
            Array.Copy(data, ret, length);
            transform(ret);
            return ret;
        }

        static public double[] Transform(double[] data)
        {
            var length = data.Length;
            var ret = new double[length];
            Array.Copy(data, ret, length);
            transform(ret);
            return ret;
        }

        static public double[] transform(double[] ret)
        {
            var length = ret.Length;
            // Hamming Window
            for (int i = 0; i < length; i++)
                ret[i] *= 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / length);
            fft(ret);
            return ret;
        }

        static void fft(double[] a)
        {
            int n = a.Length / 2;
            double theta = 2 * Math.PI / n;
            int m, mh, i, j, k, irev;
            double wr, wi, xr, xi;
        
            /* ---- scrambler ---- */
            i = 0;
            for (j = 1; j < n - 1; j++) {
                for (k = n >> 1; k > (i ^= k); k >>= 1);
                if (j < i) {
                    xr = a[2 * j];
                    xi = a[2 * j + 1];
                    a[2 * j] = a[2 * i];
                    a[2 * j + 1] = a[2 * i + 1];
                    a[2 * i] = xr;
                    a[2 * i + 1] = xi;
                }
            }
            for (mh = 1; (m = mh << 1) <= n; mh = m) {
                irev = 0;
                for (i = 0; i < n; i += m) {
                    wr = Math.Cos(theta * irev);
                    wi = Math.Sin(theta * irev);
                    for (k = n >> 2; k > (irev ^= k); k >>= 1);
                    for (j = i; j < mh + i; j++) {
                        k = j + mh;
                        xr = a[2 * j] - a[2 * k];
                        xi = a[2 * j + 1] - a[2 * k + 1];
                        a[2 * j + 0] += a[2 * k];
                        a[2 * j + 1] += a[2 * k + 1];
                        a[2 * k + 0] = wr * xr - wi * xi;
                        a[2 * k + 1] = wr * xi + wi * xr;
                    }
                }
            }
        }
    }

    class Sound : IDisposable
    {
        private CoreAudioAPI CoreAudio = new CoreAudioAPI();
        private readonly LinkedList<ArraySegment<float>> pendingSegments = new LinkedList<ArraySegment<float>>();
        private int captureUnit = 1 << 14;
        public List<float[]> capture;
        public int captureLength;

        public Sound()
        {
            CoreAudio.Start();
        }

        public void WriteBuffer()
        {
            if (pendingSegments.Count == 0)
                return;
            int maxCount = CoreAudio.BufferSize;
            int numAvailable = maxCount - CoreAudio.CurrentPadding;
            Debug.WriteLine("numAvailable: " + numAvailable);
            if (numAvailable == 0)
                return;
            IntPtr cabuf = CoreAudio.GetBuffer(numAvailable);
            var channels = CoreAudio.Format.Channels;
            var availableCount = numAvailable * channels;
            var floatSizePerByte = 4;
            var count = 0;

            while (pendingSegments.Count > 0)
            {
                var segment = pendingSegments.First.Value;
                pendingSegments.RemoveFirst();

                if (availableCount < segment.Count)
                {
                    Marshal.Copy(segment.Array, segment.Offset, cabuf, availableCount);
                    pendingSegments.AddFirst(new ArraySegment<float>(segment.Array,
                        segment.Offset + availableCount, segment.Count - availableCount));
                    count += availableCount;
                    break;
                }

                Marshal.Copy(segment.Array, segment.Offset, cabuf, segment.Count);
                cabuf = IntPtr.Add(cabuf, segment.Count * floatSizePerByte);
                count += segment.Count;
                availableCount -= segment.Count;
            }
            CoreAudio.ReleaseBuffer(count / channels, 0);
        }

        public void Add(double[] music)
        {
            float[] copy = music.Select((x) => { return (float)x; }).ToArray();
            pendingSegments.AddLast(new ArraySegment<float>(copy));
        }

        public void Add(float[] music)
        {
            pendingSegments.AddLast(new ArraySegment<float>((float[])music.Clone()));
        }

        public int SegmentCount
        {
            get { return pendingSegments.Count; }
        }

        public int CaptureMaxCount
        {
            get { return captureLength / captureUnit; }
        }

        public int CaptureUnitTime
        {
            get { return captureUnit * 1000 / SampleRate; }
        }

        public double FreqResolution
        {
            get { return SampleRate / (double)captureUnit * Channels; }
        }

        public int SampleRate
        {
            get { return CoreAudio.Format.SampleRate; }
        }

        public int Channels
        {
            get { return CoreAudio.Format.Channels; }
        }

        public async void Record(int seconds)
        {
            var buf = new float[SampleRate * Channels * seconds];
            captureLength = await CoreAudio.Record(buf);
            capture = new List<float[]>();
            for (int i = 0; i < CaptureMaxCount; i++)
            {
                var next = new float[captureUnit];
                Array.Copy(buf, i * captureUnit, next, 0, captureUnit);
                capture.Add(next);
            }
        }

        public void WriteHeader(BinaryWriter writer)
        {
            CoreAudio.Format.Serialize(writer);
        }

        public void Dispose()
        {
            CoreAudio.Stop();
            CoreAudio.Dispose();
        }
    }

    public class WaveMemoryStream : Stream
    {
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override bool CanRead { get { return true; } }
        public override long Length { get { return _waveStream.Length; } }
        public override long Position { get { return _waveStream.Position; } set { _waveStream.Position = value; } }

        private MemoryStream _waveStream;

        public WaveMemoryStream(byte[] sampleData, int audioSampleRate, ushort audioBitsPerSample, ushort audioChannels)
        {
            _waveStream = new MemoryStream();
            WriteHeader(_waveStream, sampleData.Length, audioSampleRate, audioBitsPerSample, audioChannels);
            WriteSamples(_waveStream, sampleData);
            _waveStream.Position = 0;
        }

        public void WriteHeader(Stream stream, int length, int audioSampleRate, ushort audioBitsPerSample, ushort audioChannels)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
            int fileSize = 36 + length;
            bw.Write(fileSize);
            bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bw.Write((int)16);
            bw.Write((short)1);
            bw.Write((short)audioChannels);
            bw.Write(audioSampleRate);
            bw.Write((int)(audioSampleRate * ((audioBitsPerSample * audioChannels) / 8)));
            bw.Write((short)((audioBitsPerSample * audioChannels) / 8));
            bw.Write((short)audioBitsPerSample);

            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(length);
        }

        public void WriteSamples(Stream stream, byte[] sampleData)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(sampleData, 0, sampleData.Length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _waveStream.Read(buffer, offset, count);
        }

        public virtual void WriteTo(Stream stream)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[8192];

            do
            {
                bytesRead = Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);

            stream.Flush();
        }

        public override void Flush()
        {
            _waveStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _waveStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

    class CoreAudioAPI : IDisposable
    {
        private IMMDeviceEnumerator _realEnumerator;
        private IMMDevice _realDevice;

        private IAudioClient _audioClient;
        private IAudioRenderClient _audioRenderClient;
        private WaveFormatExtensible format = new WaveFormatExtensible();
        private IAudioCaptureClient _audioCaptureClient;
        private ISimpleAudioVolume _volumeControl;

        public CoreAudioAPI()
        {
            try
            {
                _realEnumerator = new _MMDeviceEnumerator() as IMMDeviceEnumerator;

                var IID_IAudioClient = typeof(IAudioClient).GUID;
                var IID_IAudioRenderClient = typeof(IAudioRenderClient).GUID;

                if (System.Environment.OSVersion.Version.Major < 6)
                    throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
                Marshal.ThrowExceptionForHR(
                    _realEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out _realDevice));

                object obj1;
                object obj2;
                Guid guid = Guid.Empty;
                Marshal.ThrowExceptionForHR(
                    _realDevice.Activate(ref IID_IAudioClient, 0, IntPtr.Zero, out obj1));
                _audioClient = (IAudioClient)obj1;

                Marshal.ThrowExceptionForHR(
                    _audioClient.GetMixFormat(ref format));
                Debug.WriteLine(format);
                Marshal.ThrowExceptionForHR(
                    _audioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.None, 10000000, 0, format, ref guid));
                Marshal.ThrowExceptionForHR(
                    _audioClient.GetService(ref IID_IAudioRenderClient, out obj2));
                _audioRenderClient = (IAudioRenderClient)obj2;
            }
            catch(Exception e)
            {
                this.Dispose();
                throw e;
            }
        }

        public WaveFormatExtensible Format
        {
            get { return format; }
        }

        public async Task<int> Record(float[] capture)
        {
            IAudioClient audioClient = null;
            WaveFormatExtensible format = new WaveFormatExtensible();
            int capturedCount = 0;
            try
            {
                var IID_IAudioClient = typeof(IAudioClient).GUID;
                var IID_IAudioCaptureClient = typeof(IAudioCaptureClient).GUID;

                //Marshal.ThrowExceptionForHR(
                //    _realEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out realDevice));

                object obj1;
                object obj2;
                Guid guid = Guid.Empty;
                Marshal.ThrowExceptionForHR(
                    _realDevice.Activate(ref IID_IAudioClient, 0, IntPtr.Zero, out obj1));
                audioClient = (IAudioClient)obj1;

                Marshal.ThrowExceptionForHR(
                    audioClient.GetMixFormat(ref format));
                Debug.WriteLine(format);
                Marshal.ThrowExceptionForHR(
                    audioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.Loopback, 10000000, 0, format, ref guid));
                Marshal.ThrowExceptionForHR(
                        audioClient.GetService(ref IID_IAudioCaptureClient, out obj2));
                _audioCaptureClient = (IAudioCaptureClient)obj2;

                uint bufferFrameCount;
                Marshal.ThrowExceptionForHR(
                    audioClient.GetBufferSize(out bufferFrameCount));
                Debug.WriteLine("bufferFrameCount: " + bufferFrameCount.ToString());
                Marshal.ThrowExceptionForHR(
                    audioClient.Start());
                int count = 0;
                while (count < 5)
                {
                    await Task.Delay(1000);
                    var packetLength = this.NextPacketSize;
                    while (packetLength != 0)
                    {
                        Debug.WriteLine("packet exist");
                        int framesAvailable;
                        var data = GetCapturedBuffer(out framesAvailable);
                        Debug.WriteLine("captured: " + framesAvailable);
                        Marshal.Copy(data, capture, capturedCount, framesAvailable * 2);
                        capturedCount += framesAvailable * 2;
                        ReleaseCapturedBuffer(framesAvailable);
                        packetLength = this.NextPacketSize;
                        Debug.WriteLine("packetLength: " + packetLength);
                        if (capture.Length < capturedCount + packetLength)
                            return capturedCount;
                    }
                    count++;
                }
                return capturedCount;
            }
            finally
            {                Debug.WriteLine("capturedCount: " + capturedCount);
                Marshal.ThrowExceptionForHR(
                    audioClient.Stop());
                if (_audioCaptureClient != null)
                {
                    Marshal.ReleaseComObject(_audioCaptureClient);
                    _audioCaptureClient = null;
                }
                if (audioClient != null)
                    Marshal.ReleaseComObject(audioClient);
            
            }
        }

        public void ChooseSessionVolume(string appName)
        {
            try
            {
                IAudioSessionManager2 sessionManager;
                IAudioSessionEnumerator sessionEnumerator;

                var IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;

                object obj;
                Marshal.ThrowExceptionForHR(
                    _realDevice.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out obj));
                sessionManager = (IAudioSessionManager2)obj;

                sessionManager.GetSessionEnumerator(out sessionEnumerator);

                int count;
                sessionEnumerator.GetCount(out count);
                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl ctl;
                    IAudioSessionControl2 ctl2;
                    sessionEnumerator.GetSession(i, out ctl);
                    ctl2 = (IAudioSessionControl2)ctl;

                    string ir;
                    Marshal.ThrowExceptionForHR(ctl2.GetSessionIdentifier(out ir));
                    //Debug.WriteLine(ir);

                    if (ir.Contains(appName))
                    {
                        if (_volumeControl != null)
                            Marshal.ReleaseComObject(_volumeControl);
                        _volumeControl = ctl as ISimpleAudioVolume;
                        break;
                    }
                    Marshal.ReleaseComObject(ctl);
                    Marshal.ReleaseComObject(ctl2);
                }
                Marshal.ReleaseComObject(sessionEnumerator);
                Marshal.ReleaseComObject(sessionManager);
            }
            catch (Exception e)
            {
                this.Dispose();
                throw e;
            }
        }

        public float Volume
        {
            set
            {
                if (_volumeControl == null)
                    return;
                var guid = Guid.Empty;
                Marshal.ThrowExceptionForHR(
                    _volumeControl.SetMasterVolume((float)(value / 100.0f), ref guid));
            }
            get
            {
                if (_volumeControl == null)
                    return 0;
                float ret;
                Marshal.ThrowExceptionForHR(
                    _volumeControl.GetMasterVolume(out ret));
                return ret * 100;
            }
        }

        public void Dispose()
        {
            if (_realEnumerator != null)
            {
                Marshal.ReleaseComObject(_realEnumerator);
                _realEnumerator = null;
            }
            if (_realDevice != null)
            {
                Marshal.ReleaseComObject(_realDevice);
                _realDevice = null;
            }
            if (_audioClient != null)
            {
                Marshal.ReleaseComObject(_audioClient);
                _audioClient = null;
            }
            if (_audioRenderClient != null)
            {
                Marshal.ReleaseComObject(_audioRenderClient);
                _audioRenderClient = null;
            }
            if (_audioCaptureClient != null)
            {
                Marshal.ReleaseComObject(_audioCaptureClient);
                _audioCaptureClient = null;
            }
            if (_volumeControl != null)
            {
                Marshal.ReleaseComObject(_volumeControl);
                _volumeControl = null;
            }
        }

        public int BufferSize
        {
            get
            {
                uint bufferSize;
                Marshal.ThrowExceptionForHR(_audioClient.GetBufferSize(out bufferSize));
                return (int)bufferSize;
            }
        }

        public int CurrentPadding
        {
            get
            {
                int currentPadding;
                Marshal.ThrowExceptionForHR(_audioClient.GetCurrentPadding(out currentPadding));
                return currentPadding;
            }
        }

        public int NextPacketSize
        {
            get
            {
                int packetLength;
                Marshal.ThrowExceptionForHR(_audioCaptureClient.GetNextPacketSize(out packetLength));
                return packetLength;
            }
        }

        public void Start()
        {
            _audioClient.Start();
        }

        public void Stop()
        {
            _audioClient.Stop();
        }

        public void SetEventHandle(System.Threading.EventWaitHandle eventWaitHandle)
        {
            _audioClient.SetEventHandle(eventWaitHandle.SafeWaitHandle.DangerousGetHandle());
        }

        public void Reset()
        {
            _audioClient.Reset();
        }

        public IntPtr GetBuffer(int numFramesRequested)
        {
            IntPtr bufferPointer;
            Marshal.ThrowExceptionForHR(_audioRenderClient.GetBuffer(numFramesRequested, out bufferPointer));
            return bufferPointer;
        }

        public void ReleaseBuffer(int numFramesWritten, AudioClientBufferFlags bufferFlags)
        {
            Marshal.ThrowExceptionForHR(_audioRenderClient.ReleaseBuffer(numFramesWritten, bufferFlags));
        }

        public IntPtr GetCapturedBuffer(out int numFramesAvailable)
        {
            IntPtr bufferPointer;
            AudioClientBufferFlags flag;
            long devicePosition;
            long qpcPosition;
            Marshal.ThrowExceptionForHR(_audioCaptureClient.GetBuffer(out bufferPointer, out numFramesAvailable, out flag, out devicePosition, out qpcPosition));
            return bufferPointer;
        }

        public void ReleaseCapturedBuffer(int numFramesRead)
        {
            Marshal.ThrowExceptionForHR(_audioCaptureClient.ReleaseBuffer(numFramesRead));
        }

        public enum AudioSessionState
        {
            AudioSessionStateInactive = 0,
            AudioSessionStateActive = 1,
            AudioSessionStateExpired = 2
        };
        public enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2,
            ERole_enum_count = 3
        }
        public enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2,
            EDataFlow_enum_count = 3
        }

        public enum AudioClientShareMode
        {
            Shared,
            Exclusive,
        }
        [Flags]
        public enum AudioClientStreamFlags
        {
            None,
            CrossProcess = 0x00010000,
            Loopback = 0x00020000,
            EventCallback = 0x00040000,
            NoPersist = 0x00080000,
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IMMDeviceEnumerator
        {
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
        }

        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class _MMDeviceEnumerator { }

        [Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioClient
        {
            [PreserveSig]
            int Initialize(AudioClientShareMode shareMode,
                AudioClientStreamFlags streamFlags,
                long hnsBufferDuration, // REFERENCE_TIME
                long hnsPeriodicity, // REFERENCE_TIME
                [In] WaveFormatExtensible format,
                [In] ref Guid audioSessionGuid);

            int GetBufferSize(out uint bufferSize);

            [return: MarshalAs(UnmanagedType.I8)]
            long GetStreamLatency();

            int GetCurrentPadding(out int currentPadding);

            [PreserveSig]
            int IsFormatSupported(
                AudioClientShareMode shareMode,
                [In] WaveFormat pFormat,
                [Out, MarshalAs(UnmanagedType.LPStruct)] out WaveFormatExtensible closestMatchFormat);

            int GetMixFormat(ref WaveFormatExtensible waveFormat);

            // REFERENCE_TIME is 64 bit int        
            int GetDevicePeriod(out long defaultDevicePeriod, out long minimumDevicePeriod);

            int Start();

            int Stop();

            int Reset();

            int SetEventHandle(IntPtr eventHandle);

            int GetService(ref Guid interfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
        public class WaveFormatEX
        {
            public WaveFormatEncoding waveFormatTag;
            public short channels;
            public int sampleRate;
            public int averageBytesPerSecond;
            public short blockAlign;
            public short bitsPerSample;
            public short extraSize;
            public ushort wValidBitsPerSample;
            public uint dwChannelMask;
            public Guid SubFormat;

            public override string ToString()
            {
                switch (this.waveFormatTag)
                {
                    case WaveFormatEncoding.Pcm:
                    case WaveFormatEncoding.Extensible:
                        // extensible just has some extra bits after the PCM header
                        return String.Format("{0} bit PCM: {1}Hz {2} channels, {3}",
                            bitsPerSample, sampleRate, channels, SubFormat.ToString());
                    default:
                        return this.waveFormatTag.ToString();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
        public class WaveFormat
        {
            protected WaveFormatEncoding waveFormatTag;
            protected short channels;
            protected int sampleRate;
            protected int averageBytesPerSecond;
            protected short blockAlign;
            protected short bitsPerSample;
            protected short extraSize;

            public WaveFormat()
                : this(44100, 16, 2)
            {

            }

            public WaveFormat(int sampleRate, int channels)
                : this(sampleRate, 16, channels)
            {
            }

            public int ConvertLatencyToByteSize(int milliseconds)
            {
                int bytes = (int)((AverageBytesPerSecond / 1000.0) * milliseconds);
                if ((bytes % BlockAlign) != 0)
                {
                    // Return the upper BlockAligned
                    bytes = bytes + BlockAlign - (bytes % BlockAlign);
                }
                return bytes;
            }

            public static WaveFormat CreateCustomFormat(WaveFormatEncoding tag, int sampleRate, int channels, int averageBytesPerSecond, int blockAlign, int bitsPerSample)
            {
                WaveFormat waveFormat = new WaveFormat();
                waveFormat.waveFormatTag = tag;
                waveFormat.channels = (short)channels;
                waveFormat.sampleRate = sampleRate;
                waveFormat.averageBytesPerSecond = averageBytesPerSecond;
                waveFormat.blockAlign = (short)blockAlign;
                waveFormat.bitsPerSample = (short)bitsPerSample;
                waveFormat.extraSize = 0;
                return waveFormat;
            }

            public static WaveFormat CreateALawFormat(int sampleRate, int channels)
            {
                return CreateCustomFormat(WaveFormatEncoding.ALaw, sampleRate, channels, sampleRate * channels, 1, 8);
            }

            public static WaveFormat CreateMuLawFormat(int sampleRate, int channels)
            {
                return CreateCustomFormat(WaveFormatEncoding.MuLaw, sampleRate, channels, sampleRate * channels, 1, 8);
            }

            public WaveFormat(int rate, int bits, int channels)
            {
                if (channels < 1)
                {
                    throw new ArgumentOutOfRangeException("Channels must be 1 or greater", "channels");
                }
                // minimum 16 bytes, sometimes 18 for PCM
                this.waveFormatTag = WaveFormatEncoding.Pcm;
                this.channels = (short)channels;
                this.sampleRate = rate;
                this.bitsPerSample = (short)bits;
                this.extraSize = 0;

                this.blockAlign = (short)(channels * (bits / 8));
                this.averageBytesPerSecond = this.sampleRate * this.blockAlign;
            }

            public static WaveFormat MarshalFromPtr(IntPtr pointer)
            {
                WaveFormat waveFormat = (WaveFormat)Marshal.PtrToStructure(pointer, typeof(WaveFormat));
                switch (waveFormat.Encoding)
                {
                    case WaveFormatEncoding.Pcm:
                        // can't rely on extra size even being there for PCM so blank it to avoid reading
                        // corrupt data
                        waveFormat.extraSize = 0;
                        break;
                    case WaveFormatEncoding.Extensible:
                        waveFormat = (WaveFormatExtensible)Marshal.PtrToStructure(pointer, typeof(WaveFormatExtensible));
                        break;
                    case WaveFormatEncoding.Adpcm:
                        waveFormat = (AdpcmWaveFormat)Marshal.PtrToStructure(pointer, typeof(AdpcmWaveFormat));
                        break;
                    default:
                        if (waveFormat.ExtraSize > 0)
                        {
                            waveFormat = (WaveFormatExtraData)Marshal.PtrToStructure(pointer, typeof(WaveFormatExtraData));
                        }
                        break;
                }
                return waveFormat;
            }

            public static IntPtr MarshalToPtr(WaveFormat format)
            {
                int formatSize = Marshal.SizeOf(format);
                IntPtr formatPointer = Marshal.AllocHGlobal(formatSize);
                Marshal.StructureToPtr(format, formatPointer, false);
                return formatPointer;
            }

            public WaveFormat(BinaryReader br)
            {
                int formatChunkLength = br.ReadInt32();
                if (formatChunkLength < 16)
                    throw new ApplicationException("Invalid WaveFormat Structure");
                this.waveFormatTag = (WaveFormatEncoding)br.ReadUInt16();
                this.channels = br.ReadInt16();
                this.sampleRate = br.ReadInt32();
                this.averageBytesPerSecond = br.ReadInt32();
                this.blockAlign = br.ReadInt16();
                this.bitsPerSample = br.ReadInt16();
                if (formatChunkLength > 16)
                {

                    this.extraSize = br.ReadInt16();
                    if (this.extraSize > formatChunkLength - 18)
                    {
                        Console.WriteLine("Format chunk mismatch");
                        //RRL GSM exhibits this bug. Don't throw an exception
                        //throw new ApplicationException("Format chunk length mismatch");

                        this.extraSize = (short)(formatChunkLength - 18);
                    }

                    // read any extra data
                    // br.ReadBytes(extraSize);

                }
            }

            public override string ToString()
            {
                switch (this.waveFormatTag)
                {
                    case WaveFormatEncoding.Pcm:
                    case WaveFormatEncoding.Extensible:
                        // extensible just has some extra bits after the PCM header
                        return String.Format("{0} bit PCM: {1}kHz {2} channels",
                            bitsPerSample, sampleRate / 1000, channels);
                    default:
                        return this.waveFormatTag.ToString();
                }
            }

            public override bool Equals(object obj)
            {
                WaveFormat other = obj as WaveFormat;
                if (other != null)
                {
                    return waveFormatTag == other.waveFormatTag &&
                            channels == other.channels &&
                            sampleRate == other.sampleRate &&
                            averageBytesPerSecond == other.averageBytesPerSecond &&
                            blockAlign == other.blockAlign &&
                            bitsPerSample == other.bitsPerSample;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (int)waveFormatTag ^
                        (int)channels ^
                        sampleRate ^
                        averageBytesPerSecond ^
                        (int)blockAlign ^
                        (int)bitsPerSample;
            }

            public WaveFormatEncoding Encoding
            {
                get
                {
                    return waveFormatTag;
                }
            }

            public virtual void Serialize(BinaryWriter writer)
            {
                writer.Write((int)(18 + extraSize)); // wave format length
                writer.Write((short)Encoding);
                writer.Write((short)Channels);
                writer.Write((int)SampleRate);
                writer.Write((int)AverageBytesPerSecond);
                writer.Write((short)BlockAlign);
                writer.Write((short)BitsPerSample);
                writer.Write((short)extraSize);
            }

            public int Channels
            {
                get
                {
                    return channels;
                }
            }

            public int SampleRate
            {
                get
                {
                    return sampleRate;
                }
            }

            public int AverageBytesPerSecond
            {
                get
                {
                    return averageBytesPerSecond;
                }
            }

            public virtual int BlockAlign
            {
                get
                {
                    return blockAlign;
                }
            }

            public int BitsPerSample
            {
                get
                {
                    return bitsPerSample;
                }
            }

            public int ExtraSize
            {
                get
                {
                    return extraSize;
                }
            }
        }

        public enum WaveFormatEncoding : ushort
        {
            /// <summary>WAVE_FORMAT_UNKNOWN,       Microsoft Corporation</summary>
            Unknown = 0x0000,
            /// <summary>WAVE_FORMAT_PCM            Microsoft Corporation</summary>
            Pcm = 0x0001,
            /// <summary>WAVE_FORMAT_ADPCM          Microsoft Corporation</summary>
            Adpcm = 0x0002,
            /// <summary>WAVE_FORMAT_IEEE_FLOAT Microsoft Corporation</summary>
            IeeeFloat = 0x0003,
            /// <summary>WAVE_FORMAT_VSELP          Compaq Computer Corp.</summary>
            Vselp = 0x0004,
            /// <summary>WAVE_FORMAT_IBM_CVSD       IBM Corporation</summary>
            IbmCvsd = 0x0005,
            /// <summary>WAVE_FORMAT_ALAW           Microsoft Corporation</summary>
            ALaw = 0x0006,
            /// <summary>WAVE_FORMAT_MULAW          Microsoft Corporation</summary>
            MuLaw = 0x0007,
            /// <summary>WAVE_FORMAT_DTS            Microsoft Corporation</summary>
            Dts = 0x0008,
            /// <summary>WAVE_FORMAT_DRM            Microsoft Corporation</summary>
            Drm = 0x0009,
            /// <summary>WAVE_FORMAT_OKI_ADPCM      OKI</summary>
            OkiAdpcm = 0x0010,
            /// <summary>WAVE_FORMAT_DVI_ADPCM      Intel Corporation</summary>
            DviAdpcm = 0x0011,
            /// <summary>WAVE_FORMAT_IMA_ADPCM  Intel Corporation</summary>
            ImaAdpcm = DviAdpcm,
            /// <summary>WAVE_FORMAT_MEDIASPACE_ADPCM Videologic</summary>
            MediaspaceAdpcm = 0x0012,
            /// <summary>WAVE_FORMAT_SIERRA_ADPCM Sierra Semiconductor Corp </summary>
            SierraAdpcm = 0x0013,
            /// <summary>WAVE_FORMAT_G723_ADPCM Antex Electronics Corporation </summary>
            G723Adpcm = 0x0014,
            /// <summary>WAVE_FORMAT_DIGISTD DSP Solutions, Inc.</summary>
            DigiStd = 0x0015,
            /// <summary>WAVE_FORMAT_DIGIFIX DSP Solutions, Inc.</summary>
            DigiFix = 0x0016,
            /// <summary>WAVE_FORMAT_DIALOGIC_OKI_ADPCM Dialogic Corporation</summary>
            DialogicOkiAdpcm = 0x0017,
            /// <summary>WAVE_FORMAT_MEDIAVISION_ADPCM Media Vision, Inc.</summary>
            MediaVisionAdpcm = 0x0018,
            /// <summary>WAVE_FORMAT_CU_CODEC Hewlett-Packard Company </summary>
            CUCodec = 0x0019,
            /// <summary>WAVE_FORMAT_YAMAHA_ADPCM Yamaha Corporation of America</summary>
            YamahaAdpcm = 0x0020,
            /// <summary>WAVE_FORMAT_SONARC Speech Compression</summary>
            SonarC = 0x0021,
            /// <summary>WAVE_FORMAT_DSPGROUP_TRUESPEECH DSP Group, Inc </summary>
            DspGroupTruespeech = 0x0022,
            /// <summary>WAVE_FORMAT_ECHOSC1 Echo Speech Corporation</summary>
            EchoSpeechCorporation1 = 0x0023,
            /// <summary>WAVE_FORMAT_AUDIOFILE_AF36, Virtual Music, Inc.</summary>
            AudioFileAf36 = 0x0024,
            /// <summary>WAVE_FORMAT_APTX Audio Processing Technology</summary>
            Aptx = 0x0025,
            /// <summary>WAVE_FORMAT_AUDIOFILE_AF10, Virtual Music, Inc.</summary>
            AudioFileAf10 = 0x0026,
            /// <summary>WAVE_FORMAT_PROSODY_1612, Aculab plc</summary>
            Prosody1612 = 0x0027,
            /// <summary>WAVE_FORMAT_LRC, Merging Technologies S.A. </summary>
            Lrc = 0x0028,
            /// <summary>WAVE_FORMAT_DOLBY_AC2, Dolby Laboratories</summary>
            DolbyAc2 = 0x0030,
            /// <summary>WAVE_FORMAT_GSM610, Microsoft Corporation</summary>
            Gsm610 = 0x0031,
            /// <summary>WAVE_FORMAT_MSNAUDIO, Microsoft Corporation</summary>
            MsnAudio = 0x0032,
            /// <summary>WAVE_FORMAT_ANTEX_ADPCME, Antex Electronics Corporation</summary>
            AntexAdpcme = 0x0033,
            /// <summary>WAVE_FORMAT_CONTROL_RES_VQLPC, Control Resources Limited </summary>
            ControlResVqlpc = 0x0034,
            /// <summary>WAVE_FORMAT_DIGIREAL, DSP Solutions, Inc. </summary>
            DigiReal = 0x0035,
            /// <summary>WAVE_FORMAT_DIGIADPCM, DSP Solutions, Inc.</summary>
            DigiAdpcm = 0x0036,
            /// <summary>WAVE_FORMAT_CONTROL_RES_CR10, Control Resources Limited</summary>
            ControlResCr10 = 0x0037,
            /// <summary></summary>
            WAVE_FORMAT_NMS_VBXADPCM = 0x0038, // Natural MicroSystems
            /// <summary></summary>
            WAVE_FORMAT_CS_IMAADPCM = 0x0039, // Crystal Semiconductor IMA ADPCM
            /// <summary></summary>
            WAVE_FORMAT_ECHOSC3 = 0x003A, // Echo Speech Corporation
            /// <summary></summary>
            WAVE_FORMAT_ROCKWELL_ADPCM = 0x003B, // Rockwell International
            /// <summary></summary>
            WAVE_FORMAT_ROCKWELL_DIGITALK = 0x003C, // Rockwell International
            /// <summary></summary>
            WAVE_FORMAT_XEBEC = 0x003D, // Xebec Multimedia Solutions Limited
            /// <summary></summary>
            WAVE_FORMAT_G721_ADPCM = 0x0040, // Antex Electronics Corporation
            /// <summary></summary>
            WAVE_FORMAT_G728_CELP = 0x0041, // Antex Electronics Corporation
            /// <summary></summary>
            WAVE_FORMAT_MSG723 = 0x0042, // Microsoft Corporation
            /// <summary></summary>
            Mpeg = 0x0050, // WAVE_FORMAT_MPEG, Microsoft Corporation
            /// <summary></summary>
            WAVE_FORMAT_RT24 = 0x0052, // InSoft, Inc.
            /// <summary></summary>
            WAVE_FORMAT_PAC = 0x0053, // InSoft, Inc.
            /// <summary></summary>
            MpegLayer3 = 0x0055, // WAVE_FORMAT_MPEGLAYER3, ISO/MPEG Layer3 Format Tag
            /// <summary></summary>
            WAVE_FORMAT_LUCENT_G723 = 0x0059, // Lucent Technologies
            /// <summary></summary>
            WAVE_FORMAT_CIRRUS = 0x0060, // Cirrus Logic
            /// <summary></summary>
            WAVE_FORMAT_ESPCM = 0x0061, // ESS Technology
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE = 0x0062, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_CANOPUS_ATRAC = 0x0063, // Canopus, co., Ltd.
            /// <summary></summary>
            WAVE_FORMAT_G726_ADPCM = 0x0064, // APICOM
            /// <summary></summary>
            WAVE_FORMAT_G722_ADPCM = 0x0065, // APICOM
            /// <summary></summary>
            WAVE_FORMAT_DSAT_DISPLAY = 0x0067, // Microsoft Corporation
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 0x0069, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_AC8 = 0x0070, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_AC10 = 0x0071, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_AC16 = 0x0072, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_AC20 = 0x0073, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_RT24 = 0x0074, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_RT29 = 0x0075, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_RT29HW = 0x0076, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_VR12 = 0x0077, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_VR18 = 0x0078, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_TQ40 = 0x0079, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_SOFTSOUND = 0x0080, // Softsound, Ltd.
            /// <summary></summary>
            WAVE_FORMAT_VOXWARE_TQ60 = 0x0081, // Voxware Inc
            /// <summary></summary>
            WAVE_FORMAT_MSRT24 = 0x0082, // Microsoft Corporation
            /// <summary></summary>
            WAVE_FORMAT_G729A = 0x0083, // AT&T Labs, Inc.
            /// <summary></summary>
            WAVE_FORMAT_MVI_MVI2 = 0x0084, // Motion Pixels
            /// <summary></summary>
            WAVE_FORMAT_DF_G726 = 0x0085, // DataFusion Systems (Pty) (Ltd)
            /// <summary></summary>
            WAVE_FORMAT_DF_GSM610 = 0x0086, // DataFusion Systems (Pty) (Ltd)
            /// <summary></summary>
            WAVE_FORMAT_ISIAUDIO = 0x0088, // Iterated Systems, Inc.
            /// <summary></summary>
            WAVE_FORMAT_ONLIVE = 0x0089, // OnLive! Technologies, Inc.
            /// <summary></summary>
            WAVE_FORMAT_SBC24 = 0x0091, // Siemens Business Communications Sys
            /// <summary></summary>
            WAVE_FORMAT_DOLBY_AC3_SPDIF = 0x0092, // Sonic Foundry
            /// <summary></summary>
            WAVE_FORMAT_MEDIASONIC_G723 = 0x0093, // MediaSonic
            /// <summary></summary>
            WAVE_FORMAT_PROSODY_8KBPS = 0x0094, // Aculab plc
            /// <summary></summary>
            WAVE_FORMAT_ZYXEL_ADPCM = 0x0097, // ZyXEL Communications, Inc.
            /// <summary></summary>
            WAVE_FORMAT_PHILIPS_LPCBB = 0x0098, // Philips Speech Processing
            /// <summary></summary>
            WAVE_FORMAT_PACKED = 0x0099, // Studer Professional Audio AG
            /// <summary></summary>
            WAVE_FORMAT_MALDEN_PHONYTALK = 0x00A0, // Malden Electronics Ltd.
            /// <summary>WAVE_FORMAT_GSM</summary>
            Gsm = 0x00A1,
            /// <summary>WAVE_FORMAT_G729</summary>
            G729 = 0x00A2,
            /// <summary>WAVE_FORMAT_G723</summary>
            G723 = 0x00A3,
            /// <summary>WAVE_FORMAT_ACELP</summary>
            Acelp = 0x00A4,
            /// <summary></summary>
            WAVE_FORMAT_RHETOREX_ADPCM = 0x0100, // Rhetorex Inc.
            /// <summary></summary>
            WAVE_FORMAT_IRAT = 0x0101, // BeCubed Software Inc.
            /// <summary></summary>
            WAVE_FORMAT_VIVO_G723 = 0x0111, // Vivo Software
            /// <summary></summary>
            WAVE_FORMAT_VIVO_SIREN = 0x0112, // Vivo Software
            /// <summary></summary>
            WAVE_FORMAT_DIGITAL_G723 = 0x0123, // Digital Equipment Corporation
            /// <summary></summary>
            WAVE_FORMAT_SANYO_LD_ADPCM = 0x0125, // Sanyo Electric Co., Ltd.
            /// <summary></summary>
            WAVE_FORMAT_SIPROLAB_ACEPLNET = 0x0130, // Sipro Lab Telecom Inc.
            /// <summary></summary>
            WAVE_FORMAT_SIPROLAB_ACELP4800 = 0x0131, // Sipro Lab Telecom Inc.
            /// <summary></summary>
            WAVE_FORMAT_SIPROLAB_ACELP8V3 = 0x0132, // Sipro Lab Telecom Inc.
            /// <summary></summary>
            WAVE_FORMAT_SIPROLAB_G729 = 0x0133, // Sipro Lab Telecom Inc.
            /// <summary></summary>
            WAVE_FORMAT_SIPROLAB_G729A = 0x0134, // Sipro Lab Telecom Inc.
            /// <summary></summary>
            WAVE_FORMAT_SIPROLAB_KELVIN = 0x0135, // Sipro Lab Telecom Inc.
            /// <summary></summary>
            WAVE_FORMAT_G726ADPCM = 0x0140, // Dictaphone Corporation
            /// <summary></summary>
            WAVE_FORMAT_QUALCOMM_PUREVOICE = 0x0150, // Qualcomm, Inc.
            /// <summary></summary>
            WAVE_FORMAT_QUALCOMM_HALFRATE = 0x0151, // Qualcomm, Inc.
            /// <summary></summary>
            WAVE_FORMAT_TUBGSM = 0x0155, // Ring Zero Systems, Inc.
            /// <summary></summary>
            WAVE_FORMAT_MSAUDIO1 = 0x0160, // Microsoft Corporation              
            /// <summary>
            /// WAVE_FORMAT_WMAUDIO2, Microsoft Corporation
            /// </summary>
            WAVE_FORMAT_WMAUDIO2 = 0x0161,
            /// <summary>
            /// WAVE_FORMAT_WMAUDIO3, Microsoft Corporation
            /// </summary>
            WAVE_FORMAT_WMAUDIO3 = 0x0162,
            /// <summary></summary>
            WAVE_FORMAT_UNISYS_NAP_ADPCM = 0x0170, // Unisys Corp.
            /// <summary></summary>
            WAVE_FORMAT_UNISYS_NAP_ULAW = 0x0171, // Unisys Corp.
            /// <summary></summary>
            WAVE_FORMAT_UNISYS_NAP_ALAW = 0x0172, // Unisys Corp.
            /// <summary></summary>
            WAVE_FORMAT_UNISYS_NAP_16K = 0x0173, // Unisys Corp.
            /// <summary></summary>
            WAVE_FORMAT_CREATIVE_ADPCM = 0x0200, // Creative Labs, Inc
            /// <summary></summary>
            WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 0x0202, // Creative Labs, Inc
            /// <summary></summary>
            WAVE_FORMAT_CREATIVE_FASTSPEECH10 = 0x0203, // Creative Labs, Inc
            /// <summary></summary>
            WAVE_FORMAT_UHER_ADPCM = 0x0210, // UHER informatic GmbH
            /// <summary></summary>
            WAVE_FORMAT_QUARTERDECK = 0x0220, // Quarterdeck Corporation
            /// <summary></summary>
            WAVE_FORMAT_ILINK_VC = 0x0230, // I-link Worldwide
            /// <summary></summary>
            WAVE_FORMAT_RAW_SPORT = 0x0240, // Aureal Semiconductor
            /// <summary></summary>
            WAVE_FORMAT_ESST_AC3 = 0x0241, // ESS Technology, Inc.
            /// <summary></summary>
            WAVE_FORMAT_IPI_HSX = 0x0250, // Interactive Products, Inc.
            /// <summary></summary>
            WAVE_FORMAT_IPI_RPELP = 0x0251, // Interactive Products, Inc.
            /// <summary></summary>
            WAVE_FORMAT_CS2 = 0x0260, // Consistent Software
            /// <summary></summary>
            WAVE_FORMAT_SONY_SCX = 0x0270, // Sony Corp.
            /// <summary></summary>
            WAVE_FORMAT_FM_TOWNS_SND = 0x0300, // Fujitsu Corp.
            /// <summary></summary>
            WAVE_FORMAT_BTV_DIGITAL = 0x0400, // Brooktree Corporation
            /// <summary></summary>
            WAVE_FORMAT_QDESIGN_MUSIC = 0x0450, // QDesign Corporation
            /// <summary></summary>
            WAVE_FORMAT_VME_VMPCM = 0x0680, // AT&T Labs, Inc.
            /// <summary></summary>
            WAVE_FORMAT_TPC = 0x0681, // AT&T Labs, Inc.
            /// <summary></summary>
            WAVE_FORMAT_OLIGSM = 0x1000, // Ing C. Olivetti & C., S.p.A.
            /// <summary></summary>
            WAVE_FORMAT_OLIADPCM = 0x1001, // Ing C. Olivetti & C., S.p.A.
            /// <summary></summary>
            WAVE_FORMAT_OLICELP = 0x1002, // Ing C. Olivetti & C., S.p.A.
            /// <summary></summary>
            WAVE_FORMAT_OLISBC = 0x1003, // Ing C. Olivetti & C., S.p.A.
            /// <summary></summary>
            WAVE_FORMAT_OLIOPR = 0x1004, // Ing C. Olivetti & C., S.p.A.
            /// <summary></summary>
            WAVE_FORMAT_LH_CODEC = 0x1100, // Lernout & Hauspie
            /// <summary></summary>
            WAVE_FORMAT_NORRIS = 0x1400, // Norris Communications, Inc.
            /// <summary></summary>
            WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 0x1500, // AT&T Labs, Inc.
            /// <summary></summary>
            WAVE_FORMAT_DVM = 0x2000, // FAST Multimedia AG
            /// <summary>WAVE_FORMAT_EXTENSIBLE</summary>
            Extensible = 0xFFFE, // Microsoft
            /// <summary></summary>
            WAVE_FORMAT_DEVELOPMENT = 0xFFFF,

            // others - not from MS headers
            /// <summary>WAVE_FORMAT_VORBIS1 "Og" Original stream compatible</summary>
            Vorbis1 = 0x674f,
            /// <summary>WAVE_FORMAT_VORBIS2 "Pg" Have independent header</summary>
            Vorbis2 = 0x6750,
            /// <summary>WAVE_FORMAT_VORBIS3 "Qg" Have no codebook header</summary>
            Vorbis3 = 0x6751,
            /// <summary>WAVE_FORMAT_VORBIS1P "og" Original stream compatible</summary>
            Vorbis1P = 0x676f,
            /// <summary>WAVE_FORMAT_VORBIS2P "pg" Have independent headere</summary>
            Vorbis2P = 0x6770,
            /// <summary>WAVE_FORMAT_VORBIS3P "qg" Have no codebook header</summary>
            Vorbis3P = 0x6771,

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
        public class WaveFormatExtensible : WaveFormat
        {
            short wValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0
            int dwChannelMask; // which channels are present in stream
            Guid subFormat;

            public WaveFormatExtensible()
            {
            }

            public WaveFormatExtensible(int rate, int bits, int channels)
                : base(rate, bits, channels)
            {
                waveFormatTag = WaveFormatEncoding.Extensible;
                extraSize = 22;
                wValidBitsPerSample = (short)bits;
                for (int n = 0; n < channels; n++)
                {
                    dwChannelMask |= (1 << n);
                }
                if (bits == 32)
                {
                    //KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
                    //subFormat = AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT; // new Guid("00000003-0000-0010-8000-00aa00389b71");
                    subFormat = new Guid("00000003-0000-0010-8000-00aa00389b71");
                }
                else
                {
                    //KSDATAFORMAT_SUBTYPE_PCM
                    //subFormat = AudioMediaSubtypes.MEDIASUBTYPE_PCM; // new Guid("00000001-0000-0010-8000-00aa00389b71");
                    subFormat = new Guid("00000001-0000-0010-8000-00aa00389b71");
                }

            }

            public override void Serialize(System.IO.BinaryWriter writer)
            {
                base.Serialize(writer);
                writer.Write(wValidBitsPerSample);
                writer.Write(dwChannelMask);
                byte[] guid = subFormat.ToByteArray();
                writer.Write(guid, 0, guid.Length);
            }

            public override string ToString()
            {
                return String.Format("{0} wBitsPerSample:{1} dwChannelMask:{2} subFormat:{3} extraSize:{4}",
                    base.ToString(),
                    wValidBitsPerSample,
                    dwChannelMask,
                    subFormat,
                    extraSize);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
        class WaveFormatExtraData : WaveFormat
        {
            // try with 100 bytes for now, increase if necessary
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            byte[] extraData = new byte[100];

            /// <summary>
            /// parameterless constructor for marshalling
            /// </summary>
            WaveFormatExtraData()
            {
            }

            public WaveFormatExtraData(BinaryReader reader)
                : base(reader)
            {
                if (this.extraSize > 0)
                {
                    reader.Read(extraData, 0, extraSize);
                }
            }

            public override void Serialize(BinaryWriter writer)
            {
                base.Serialize(writer);
                if (extraSize > 0)
                {
                    writer.Write(extraData, 0, extraSize);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public class AdpcmWaveFormat : WaveFormat
        {
            short samplesPerBlock;
            short numCoeff;
            // 7 pairs of coefficients
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            short[] coefficients;

            AdpcmWaveFormat()
                : this(8000, 1)
            {
            }

            public int SamplesPerBlock
            {
                get { return samplesPerBlock; }
            }

            public int NumCoefficients
            {
                get { return numCoeff; }
            }

            public short[] Coefficients
            {
                get { return coefficients; }
            }

            public AdpcmWaveFormat(int sampleRate, int channels) :
                base(sampleRate, 0, channels)
            {
                this.waveFormatTag = WaveFormatEncoding.Adpcm;

                // TODO: validate sampleRate, bitsPerSample
                this.extraSize = 32;
                switch (this.sampleRate)
                {
                    case 8000:
                    case 11025:
                        blockAlign = 256;
                        break;
                    case 22050:
                        blockAlign = 512;
                        break;
                    case 44100:
                    default:
                        blockAlign = 1024;
                        break;
                }

                this.bitsPerSample = 4;
                this.samplesPerBlock = (short)((((blockAlign - (7 * channels)) * 8) / (bitsPerSample * channels)) + 2);
                this.averageBytesPerSecond =
                    ((this.SampleRate * blockAlign) / samplesPerBlock);

                // samplesPerBlock = blockAlign - (7 * channels)) * (2 / channels) + 2;


                numCoeff = 7;
                coefficients = new short[14] {
                    256,0,512,-256,0,0,192,64,240,0,460,-208,392,-232
                };
            }

            public override void Serialize(System.IO.BinaryWriter writer)
            {
                base.Serialize(writer);
                writer.Write(samplesPerBlock);
                writer.Write(numCoeff);
                foreach (short coefficient in coefficients)
                {
                    writer.Write(coefficient);
                }
            }

            public override string ToString()
            {
                return String.Format("Microsoft ADPCM {0} Hz {1} channels {2} bits per sample {3} samples per block",
                    this.SampleRate, this.channels, this.bitsPerSample, this.samplesPerBlock);
            }
        }

        [Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioRenderClient
        {
            int GetBuffer(int numFramesRequested, out IntPtr dataBufferPointer);
            int ReleaseBuffer(int numFramesWritten, AudioClientBufferFlags bufferFlags);
        }

        [Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IAudioCaptureClient
        {
            /*HRESULT GetBuffer(
                BYTE** ppData,
                UINT32* pNumFramesToRead,
                DWORD* pdwFlags,
                UINT64* pu64DevicePosition,
                UINT64* pu64QPCPosition
                );*/

            int GetBuffer(
                out IntPtr dataBuffer,
                out int numFramesToRead,
                out AudioClientBufferFlags bufferFlags,
                out long devicePosition,
                out long qpcPosition);

            int ReleaseBuffer(int numFramesRead);

            int GetNextPacketSize(out int numFramesInNextPacket);
        }

        [Flags]
        public enum AudioClientBufferFlags
        {
            None,
            DataDiscontinuity = 0x1,
            Silent = 0x2,
            TimestampError = 0x4
        }

        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionManager2
        {
            int NotImpl1();
            int NotImpl2();

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

            // the rest is not implemented
        }

        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);

            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl Session);
        }

        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl
        {
        }

        [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl2
        {
            [PreserveSig]
            int GetState(out AudioSessionState state);
            [PreserveSig]
            int GetDisplayName([Out(), MarshalAs(UnmanagedType.LPWStr)] out string name);
            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string value, Guid EventContext);
            [PreserveSig]
            int GetIconPath([Out(), MarshalAs(UnmanagedType.LPWStr)] out string Path);
            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, Guid EventContext);
            [PreserveSig]
            int GetGroupingParam(out Guid GroupingParam);
            [PreserveSig]
            int SetGroupingParam(Guid Override, Guid Eventcontext);
            [PreserveSig]
            int NotImpl1();
            [PreserveSig]
            int NotImpl2();
            [PreserveSig]
            int GetSessionIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
            [PreserveSig]
            int GetSessionInstanceIdentifier([Out(), MarshalAs(UnmanagedType.LPWStr)] out string retVal);
            [PreserveSig]
            int GetProcessId(out UInt32 retvVal);
            [PreserveSig]
            int IsSystemSoundsSession();
            [PreserveSig]
            int SetDuckingPreference(bool optOut);
        }

        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, ref Guid EventContext);

            [PreserveSig]
            int GetMasterVolume(out float pfLevel);

            [PreserveSig]
            int SetMute(bool bMute, ref Guid EventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);
        }

    }

}
