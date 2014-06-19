using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Ochawan
{
    public partial class ViewForm : Form
    {
        bool isDrawing = false;
        bool isDrawCurLine = false;

        ComeLang comelang;
        Timer advTimer;
        Timer clearTimer;
        Color tranceparentColor;
        static int fontSize = 12;
        static int lineHeight = fontSize + 6;
        static int enumLines = 24;
        const int paddingLines = 11;
        static Font midmsfont = new Font("MS UI Gothic", fontSize);
        static Font bigmsfont = new Font("MS UI Gothic", 24);
        static Font curfont = midmsfont;
        static StringFormat format;
        List<string> code;
        List<int> codeDepth;
        int codeLength;
        int curLine;
        int deltaY;
        string loadfile;
        bool show_msg;

        public ViewForm()
        {
            InitializeComponent();

            comelang = new ComeLang(this);
            advTimer = new Timer() { Interval = 60 };
            clearTimer = new Timer();

            advTimer.Tick += new EventHandler(timer_Tick);
            clearTimer.Tick += new EventHandler(clearEvent);

            this.DoubleBuffered = true;
            tranceparentColor = this.BackColor;
            format = new StringFormat();
            format.Alignment = StringAlignment.Far;
            LoadFile("GameForm.cs");
            Term.InitVars();
        }

        public void Exec(string cmd, bool come)
        {
            show_msg = false;

            Func<string, Func<string, bool>, bool> rewrite = (ln, func) =>
            {
                if (Regex.IsMatch(ln, @"\d") && func(cmd))
                {
                    ReadFile(come);
                    ShowLine(int.Parse(ln));
                    return true;
                }
                return false;
            };

            if (comelang.IsRun)
            {
                if (cmd.StartsWith("stop"))
                {
                    comelang.Stop();
                    this.Invalidate();
                }
                else if (cmd.StartsWith("input"))
                {
                    comelang.Input(cmd.Substring(6));
                }
                return;
            }
            else if (comelang.IsActive)
            {
                comelang.Inactivate();
            }

            int line;
            if (int.TryParse(cmd, out line))
            {
                ReadFile(come);
                ShowLine(line);
            }
            else if (cmd.StartsWith("list"))
            {
                int ln;
                ReadFile(come);
                if (int.TryParse(cmd.Substring(4), out ln))
                    ListCode(ln);
                else
                    ListCode();
            }
            else if (cmd.StartsWith("load") && !come)
            {
                var file = cmd.Substring(4).Trim();
                if (LoadFile(file))
                    ShowMessage(file + "をロードしました", true);
                else
                    ShowMessage(file + "をロードできませんでした");
            }
            else if (come)
            {
                string msg;
                var words = cmd.Split(' ');
                switch (words[0])
                {
                    case "run":
                        comelang.Run();
                        return;
                    case "stop":
                        return; // run してないときは何もしない
                    case "write":
                        if (rewrite(words[1], comelang.Write)) return;
                        else return;
                    case "insert":
                        if (rewrite(words[1], comelang.Insert)) return;
                        else return;
                    case "delete":
                        if (rewrite(words[1], comelang.Delete)) return;
                        else msg = "削除失敗しました";
                        break;
                    case "add":
                        if (comelang.Add(cmd))
                        {
                            ReadFile(come);
                            ShowLine(comelang.sentences.Count);
                        }
                        return;
                    case "input":
                        msg = "停止中です";
                        break;
                    case "load":
                        if (comelang.Load())
                            msg = "ロードしました";
                        else
                            msg = "ロード失敗しました";
                        break;
                    case "save":
                        comelang.Save();
                        msg = "セーブしました";
                        break;
                    default:
                        msg = "実行失敗しました";
                        break;
                }
                ShowMessage(msg);
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            deltaY--;
            if (deltaY <= 0)
            {
                if (curLine + enumLines / 2 > codeLength + 5)
                {
                    advTimer.Stop();
                    clearTimer.Interval = 5000;
                    clearTimer.Start();
                }
                else
                {
                    curLine++;
                    deltaY = lineHeight;
                }
            }

            this.Invalidate();
        }

        void clearEvent(object sender, EventArgs e)
        {
            this.BackColor = tranceparentColor;
            isDrawing = false;
            clearTimer.Stop();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var grfx = e.Graphics;

            if (show_msg)
            {
                this.BackColor = Color.DarkRed;
                grfx.DrawString(code[0], bigmsfont, Brushes.Black, 80, 150);
                return;
            }

            if (comelang.IsActive)
            {
                grfx.DrawImage(comelang.bmp, 0, 0);
                return;
            }

            grfx.FillEllipse(Brushes.Blue, 10, 10, 10, 10);

            if (!isDrawing)
                return;
            var detum = Math.Max(curLine, enumLines / 2);
            for (int i = 0; i < enumLines; i++)
            {
                var line = i - enumLines / 2  + detum;
                var y = i * lineHeight + deltaY - lineHeight / 2;
                grfx.DrawString((line + 1).ToString(), curfont, Brushes.White, 50, y, format);
                grfx.DrawString(code[line], curfont, Brushes.White, 80 + 20 * codeDepth[line], y);
            }
            var refLine = Math.Min(curLine, enumLines / 2);
            var ht = refLine * lineHeight + deltaY - lineHeight / 2;
            if (isDrawCurLine)
                grfx.DrawLine(Pens.Red, 0, ht, this.Width, ht);

        }

        void ListCode(int line = 0)
        {
            if (line < 0)
                line += codeLength;
            if (line < 0 || line > codeLength)
                return;
            deltaY = lineHeight;
            curLine = Math.Max(line, enumLines / 2);
            this.BackColor = Color.DarkSlateBlue;
            isDrawing = true;
            isDrawCurLine = false;
            clearTimer.Stop();
            this.Invoke((Action)(()=>{advTimer.Start();}));
        }

        bool LoadFile(string file)
        {
            var mc = new Regex(@"[\\/]");
            if (mc.IsMatch(file))
                return false;
            if (!File.Exists("../../" + file))
                return false;
            loadfile = file;
            return true;
        }

        void ShowMessage(string msg, bool success = false)
        {
            if (success)
                this.BackColor = Color.Navy;
            else
                this.BackColor = Color.DarkRed;
            show_msg = true;
            code = new List<string>();
            code.Add(msg);
            codeLength = 1;
            this.Invalidate();
        }

        void ReadFile(bool come)
        {
            if (come)
            {
                codeLength = comelang.sentences.Count;
                code = comelang.sentences.ToList();
                codeDepth = comelang.codeDepth.ToList();
            }
            else
            {
                var path = "../../" + loadfile;
                var lines = File.ReadAllLines(path);
                codeLength = lines.Length;
                code = new List<string>(lines);
                codeDepth.AddRange(new int[codeLength]);
            }
            code.AddRange(new string[enumLines]);
            codeDepth.AddRange(new int[enumLines]);
        }

        void ShowLine(int line)
        {
            if (line < 0)
                line += codeLength;
            if (line < 0 || line > codeLength)
                return;
            deltaY = lineHeight / 2;
            curLine = line;
            this.BackColor = Color.DarkSlateBlue;
            isDrawing = true;
            isDrawCurLine = true;
            advTimer.Stop();
            clearTimer.Interval = 10000;
            clearTimer.Stop();
            this.Invoke((Action)(() => { clearTimer.Start(); }));

            this.Invalidate();
            //this.Update();
        }

        private void ViewForm_Load(object sender, EventArgs e)
        {
            test();
        }

        void test()
        {
            var testcode = new string[] {
                "var width = 9",
                "var height = 9",
                "var numMine = 10",
                "var numCell = width * height - 1",
                "var cells = [ 0 .. numCell ]",
                "var mineNums = clone cells",
                "var n = 0",
                "while n < numMine",
                    "var rd = random numCell",
                    "if cells !! rd != -1",
                        "cells rd = -1",
                        "n = n + 1",
                    "end",
                "end",
                "func countAt pos : num -> num",
                    "var count = 0",
                    "return -1 if cells !! pos == -1",
                    "",
                    "var dr = width + 1",
                    "var dl = width - 1",
                    "for var d in [ -1 1 width -width dr -dr dl -dl ]",
                        "var dp = pos + d",
                        "count = count + 1 if dp > 0 && dp < numCell && cells !! dp == -1",
                    "end",
                    "return count",
                "end",
                "map mineNums countAt item",
                "for var ix in [ 0 .. numCell ]",
                    "var val = mineNums !! ix",
                    "var x = ix % width * 20 + 20",
                    "var y = ( ix - ix % width ) / width * 20 + 20",
                    "print ( str val ) x y",
                "end",
                "var fill = fillrect 10 10",
                "for var ix in [ 0 .. numCell ]",
                    "var val = mineNums !! ix",
                    "var x = ix % width * 20 + 20",
                    "var y = ( ix - ix % width ) / width * 20 + 20",
                    "fill x y",
                "end",
                "invalidate"
            };
            foreach (var str in testcode)
                Exec("add " + str, true);
            Exec("run", true);
        }

    }

    class ComeLang
    {
        Graphics grfx;
        ViewForm form;
        Parser parser = new Parser();
        Stack<Parser> nestFunc = new Stack<Parser>();
        Font msfont = new Font("MS UI Gothic", 14, FontStyle.Bold);
        public Bitmap bmp = new Bitmap(640, 360);
        public List<string> sentences = new List<string>();
        public List<int> codeDepth = new List<int>();
        public Queue<string> InQueue = new Queue<string>();
        int curDepth = 0;
        bool isRun = false;
        bool isActive = false;

        public ComeLang(ViewForm form)
        {
            grfx = Graphics.FromImage(bmp);
            this.form = form;
            ComeFunc.Init(grfx, this, form);
        }

        public bool IsRun { get { return isRun; } }
        public bool IsActive { get { return isActive; } }

        public void Inactivate() { isActive = false; }

        #region command

        bool write(string cmd, Action<string, int> action)
        {
            var words = cmd.Split(' ');
            var line = int.Parse(words[1]) - 1;
            check(line, words.Skip(2));
            if (sentences[0] != null && line >= 0 && line < sentences.Count)
            {
                action(string.Join(" ", words.Skip(2)), line);
                return true;
            }
            return false;
        }

        public bool Write(string cmd)
        {
            return write(cmd, (com, line) => { sentences[line] = com; });
        }

        public bool Insert(string cmd)
        {
            return write(cmd, (com, line) => { sentences.Insert(line, com); });
        }

        public bool Delete(string cmd)
        {
            return write(cmd, (com, line) => { sentences.RemoveAt(line); });
        }

        public bool Add(string cmd)
        {
            sentences.Add("");
            var success = write(cmd.Insert(3, " " + sentences.Count), (com, line) => {
                sentences[line] = com;
            });

            if (success)
                return true;
            else
            {
                sentences.RemoveAt(sentences.Count - 1);
                return false;
            }
        }

        public void Input(string data)
        {
            InQueue.Enqueue(data);
        }

        public async void Run()
        {
            isRun = true;
            isActive = true;
            InQueue.Clear();
            Term.InitVars();
            var line = 0;
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    grfx.Clear(Color.White);
                    while (isRun)
                    {
                        line = parse(line);
                        if (line == -1)
                            throw new Exception("return from global region");
                        if (line >= sentences.Count)
                        {
                            isRun = false;
                            if (parser.NestLevel != 0)
                                throw new Exception(" \"end\"  is not found");
                        }
                    }
                }
                catch (Exception e)
                {
                    showException(e, line);
                }
            });
        }

        public async void Stop()
        {
            isRun = false;
            await Task.Delay(3000);
            isActive = false;
        }

        public void Save()
        {
            File.WriteAllLines("Come.txt", sentences);
        }

        public bool Load()
        {
            var file = "Come.txt";
            if (File.Exists(file))
            {
                sentences = new List<string>(File.ReadAllLines(file));
                return true;
            }
            return false;
        }


        #endregion

        void showException (Exception e, int line)
        {
            Stop();
            grfx.Clear(Color.DarkRed);
            var width = 30;
            var count = Math.Min(e.Message.Length, width);

            if (e.GetType() == typeof(FuncException))
                line = ((FuncException)e).line;

            grfx.DrawString((line + 1) + "行目：" + e.Message.Substring(0, count), msfont, Brushes.Black, 10, 100);
            if (e.Message.Length > width)
                grfx.DrawString(e.Message.Substring(width), msfont, Brushes.Black, 100, 130);
            grfx.DrawString(sentences[line], msfont, Brushes.Black, 20, 160);

            form.Invalidate();
        }

        void check(int line, IEnumerable<string> words)
        {
            if (words.Count() == 0)
                return;

            grfx.Clear(Color.DarkRed);
            try
            {
                switch (words.ElementAt(0))
                {
                    case "var":
                        parser.Var(words.ToArray());
                        codeDepth.Add(curDepth);
                        break;
                    case "for":
                    case "func":
                    case "while":
                    case "until":
                    case "if":
                    case "unless":
                        codeDepth.Add(curDepth);
                        curDepth++;
                        break;
                    case "elsif":
                    case "else":
                        codeDepth.Add(curDepth - 1);
                        break;
                    case "end":
                        curDepth--;
                        codeDepth.Add(curDepth);
                        break;
                    default:
                        codeDepth.Add(curDepth);
                        parser.Check(words.ToArray());
                        break;
                }
            }
            catch (Exception e)
            {
                grfx.Clear(Color.DarkRed);
                grfx.DrawString(e.Message, msfont, Brushes.Black, 10, 100);
                form.Invalidate();
                isActive = true;
            }
        }

        int parse(int line)
        {
            if (sentences[line] == "")
                return line + 1;

            var words = sentences[line].Split(' ');
            
           switch (words[0])
            {
                case "map":    parser.Map(words); break;
                case "for":    parser.For(words, line); break;
                case "func":   parser.Func(words, line); break;
                case "var":    parser.Var(words); break;
                case "while":  parser.While(words, line); break;
                case "until":  parser.While(words, line, true); break;
                case "if":     parser.If(words); break;
                case "unless": parser.If(words, true); break;
                case "elsif":  parser.Elsif(words); break;
                case "else":   parser.Else(); break;
                case "return": return parser.Return(words, line);
                case "end":
                    var next = parser.Next();
                    if (next == -1)
                        return line + 1;
                    var sen = sentences[next].Split(' ');
                    if (sen[0] == "func")
                        Term.SetFuncEnd(sen[1], line);
                    else if (sen[0] == "for")
                        return parser.ForNext(sen, next, line);
                    else
                        return next;
                    break;
                default:
                    parser.Sentence(words);
                    break;
            }
            
            return line + 1;
        }

        public object DefineFunc(Term func)
        {
            var line = func.info.begin;
            var end = func.info.end;
            Term.SettingLocal(func.info, func.args);
            nestFunc.Push(parser);
            parser = new Parser();
            try
            {
                while (isRun)
                {
                    line = parse(line);
                    if (line == -1)
                        return ComeFunc.Result;
                    if (line == end)
                        return Term.nil;
                    if (line >= sentences.Count)
                        isRun = false;
                }
                throw new Exception("function is not terminated");
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FuncException))
                    line = ((FuncException)e).line;
                throw new FuncException(e, line);
            }
            finally
            {
                parser = nestFunc.Pop();
                Term.ReleaseLocal();
            }
        }
    }

    class Parser
    {
        public static Regex signs = new Regex(@"[\-+*/:?%&|#!<>@=]+");
        public static Regex termPatt = new Regex(@"-?[A-Za-z0-9]+|^""$""|\(|\$"); 
        int index;
        string[] tokens;
        bool isExec;
        public int NestLevel = 0;
        bool ignore = false;
        bool ignoreElse = false;
        Stack<int> nests = new Stack<int>();
        Stack<int> whileLines = new Stack<int>();
        Stack<Tuple<Term, int>> forCollection = new Stack<Tuple<Term, int>>();
        Func<object, string, object, object> operation;
        static Dictionary<string, int> opOrder = 
            new Dictionary<string, int> { {"!!", 8},
                                          {"*", 7}, {"/", 7}, {"%", 7}, 
                                          {"+", 6}, {"-", 6},
                                          {"++",5},
                                          {">", 4}, {">=", 4},{"<", 4},{"<=", 4},
                                          {"==", 3}, {"!=", 3},
                                          {"&&", 2}, 
                                          {"||", 1},
        };
        string curToken { get { return tokens[index]; } }
        string nexToken { get { 
            if (index < tokens.Length - 1)
                return tokens[index + 1];
            else
                return ""; } }

        public Type Check(string[] tokens, int ix = 0)
        {
            this.tokens = tokens;
            index = ix;
            isExec = false;
            operation = checkOp;
            var ret = expr();
            return (Type)ret;
        }

        public object Expr(string[] tokens, int ix = 0)
        {
            this.tokens = tokens;
            index = ix;
            isExec = true;
            operation = doOp;

            return expr();
        }

        public void CheckSentence(string[] tokens)
        {

        }

        public void Sentence(string[] tokens)
        {
            if (ignore)
                return;

            tokens = onCond(tokens);
            if (tokens != null)
                assignSentence(tokens);
        }

        string[] onCond(string[] tokens)
        {
            int ix;
            bool cond;
            string[] ret = null;
            if (tokens.Contains("if"))
            {
                ix = Array.IndexOf(tokens, "if");
                cond = (bool)Expr(tokens, ix + 1);
                if (cond)
                    ret = tokens.Take(ix).ToArray();
            }
            else if (tokens.Contains("unless"))
            {
                ix = Array.IndexOf(tokens, "unless");
                cond = (bool)Expr(tokens, ix + 1);
                if (!cond)
                    ret = tokens.Take(ix).ToArray();
            }
            else
                ret = tokens;

            return ret;
        }

        void assignSentence(string[] tokens)
        {
            if (tokens.Length > 2 && tokens[1] == "=")
            {
                Term.Assign(tokens[0], Expr(tokens, 2), Check(tokens, 2));
            }
            else if (tokens.Length > 3 && tokens[2] == "=")
            {
                var term = (Term)Term.Parse(tokens[0], true);
                var type = (Type)Term.Parse(tokens[0], false);
                var index = (float)Term.Parse(tokens[1], true);
                if (type == Check(tokens, 3))
                    term.List[(int)index] = Expr(tokens, 3);
                else
                    throw new Exception("type mismatch: element is " + type.Name +
                        ", but expression is " + Check(tokens, 3));
            }
            else
                Expr(tokens);
        }

        public void Map(string[] tokens)
        {
            if (ignore)
                return;
            var term = (Term)Term.Parse(tokens[1], true);
            var type = (Type)Term.Parse(tokens[1], false);
            var list = term.List;
            for (var i = 0; i < list.Count; i++)
            {
                Term.RegistVar("item", list[i], type);
                var ret = Expr(tokens, 2);
                if (ret != null)
                    list[i] = ret;
            }
        }

        public void For(string[] tokens, int line)
        {
            NestLevel++;
            if (ignore)
                return;
            if (tokens[1] != "var" || tokens[3] != "in")
                throw new Exception("syntax error: for-in");
            nests.Push(NestLevel);
            
            var collect = (Term)Expr(tokens, 4);
            if (collect.Length > 0)
            {
                Term.RegistVar(tokens[2], collect.At(0), collect.type);
                forCollection.Push(new Tuple<Term, int>(collect, 1));
                whileLines.Push(line);
            }
            else
            {
                whileLines.Push(-1);
                ignore = true;
            }
        }

        public int ForNext(string[] tokens, int next, int line)
        {
            var collection = forCollection.Pop();
            var collect = collection.Item1;
            var count = collection.Item2;
            if (collect.Length > count)
            {
                NestLevel++;
                nests.Push(NestLevel);
                Term.RegistVar(tokens[2], collect.At(count), collect.type);
                forCollection.Push(new Tuple<Term, int>(collect, count + 1));
                whileLines.Push(next);
                return next + 1;
            }
            else
            {
                return line + 1;
            }

        }

        public void Func(string[] tokens, int line)
        {
            if (NestLevel != 0 || nests.Count != 0 || ignore)
                throw new Exception("function can not define at this context");
            NestLevel++;
            nests.Push(NestLevel);

            var name = tokens[1];
            var argnames = new List<string>();
            var argtypes = new List<Type>();
            var i = 2;
            while(tokens[i] != "->")
            {
                argnames.Add(tokens[i]);
                argtypes.Add(TypeOf(tokens[i + 2]));
                i += 3;
                if (i > tokens.Length)
                    throw new Exception("funtion definition need \"->\"");
            }
            argtypes.Add(TypeOf(tokens[i + 1]));
            var info = new FuncInfo(line + 1, -1, argnames.ToArray(), argtypes.ToArray());
            Term.RegistVar(name, new Term(name, info), typeof(ComeFunc));

            ignore = true;
            whileLines.Push(line);
        }

        Type TypeOf(string type)
        {
            switch (type)
            {
                case "void": return typeof(void);
                case "num": return typeof(float);
                case "str": return typeof(string);
                case "list": return typeof(List<object>);
                default: throw new Exception("no type");
            }
        }

        public int Return(string[] tokens, int line)
        {
            if (ignore)
                return line + 1;

            tokens = onCond(tokens);
            if (tokens == null)
                return line + 1;
            ComeFunc.Result = Expr(tokens, 1);
            return -1;
        }

        public void Var(string[] tokens)
        {
            if (ignore)
                return;
            var name = tokens[1];
            if (tokens[2] != "=")
                throw new Exception("syntax error: assignment");
            Term.RegistVar(name, Expr(tokens, 3), Check(tokens, 3));
        }

        public void While(string[] tokens, int line, bool unless = false)
        {
            NestLevel++;
            if (ignore)
                return;
            nests.Push(NestLevel);
            var cond = (bool)Expr(tokens, 1);
            if (unless)
                cond = !cond;
            if (cond)
                whileLines.Push(line);
            else
                whileLines.Push(-1);
            ignore = !cond;
        }

        public void If(string[] tokens, bool unless = false)
        {
            NestLevel++;
            if (ignore)
                return;
            nests.Push(NestLevel);
            var cond = (bool)Expr(tokens, 1);
            if (unless)
                cond = !cond;
            ignoreElse = cond;
            ignore = !cond;
            whileLines.Push(-1);
        }

        public void Elsif(string[] tokens)
        {
            ignore = true;
            if (ignoreElse || nests.First() != NestLevel)
                return;
            var cond = (bool)Expr(tokens, 1);
            if (!cond)
                return;
            ignore = false;
            ignoreElse = true;
        }

        public void Else() { Elsif(null); }

        public int Next()
        {
            if (nests.Count == 0)
                throw new Exception("endに対応する文がありません");
            var nest = nests.First();
            if (nest == NestLevel)
            {
                ignore = false;
                ignoreElse = true;
                nests.Pop();
                NestLevel--;
                return whileLines.Pop();
            }
            else if (nest < NestLevel)
            {
                NestLevel--;
                return -1;
            }
            else
                throw new Exception("syntax error");
        }

        /************************************************
         
           expr : fexp (op fexp)*
           fexp : aexp (aexp)*
           aexp : var | literal | '(' expr ')' | list
          
         ************************************************/

        object expr()
        {
            var stack = new Stack<object>();
            stack.Push(fexp());
            if (signs.IsMatch(nexToken))
            {
                stack.Push(nexToken);
                index++; // op
                index++; // right hand
                stack.Push(fexp());
            }

            while (signs.IsMatch(nexToken))
            {
                var rh = stack.Pop();
                var op = (string)stack.Pop();
                var lh = stack.Pop();
                if (op == "&&" && !(bool)lh || op == "||" && (bool)lh)
                    return lh;

                if (opOrder[op] >= opOrder[nexToken])
                {
                    stack.Push(operation(lh, op, rh));
                }
                else
                {
                    stack.Push(lh);
                    stack.Push(op);
                    stack.Push(rh);
                }
                stack.Push(nexToken);
                index++; // op
                index++; // right hand
                stack.Push(fexp());
            }

            var ret = stack.Pop();
            while(stack.Count > 0)
            {
                var op = (string)stack.Pop();
                ret = operation(stack.Pop(), op, ret);
            }
            return ret;
        }

        object fexp()
        {
            var exp = aexp();
            var isApp = true;
            while(termPatt.IsMatch(nexToken))
            {
                index++;
                isApp = Term.App(ref exp, aexp(), isExec);
            }
            if (!isApp && !isExec)
                return typeof(ComeFunc);
            return exp;
        }

        object aexp()
        {
            if (curToken == "(")
            {
                index++; // skip "("
                var ret = expr();
                index++; // skip ")"
                return ret;
            }
            else if (curToken == "$")
            {
                index++; // skip "$"
                return expr();
            }
            else if (curToken == "[")
            {
                index++;
                var type = Term.Parse(curToken, false);

                if (!isExec)
                {
                    while (curToken != "]")
                        index++;
                    return type;
                }

                var ret = new List<object>();
                while(curToken != "]")
                {
                    var ele = Term.Parse(curToken, isExec);
                    ret.Add(ele);
                    index++;
                    if (curToken == "..")
                    {
                        var end = (float)Term.Parse(nexToken, isExec);
                        var el = (float)ele;
                        if (el < end)
                            while (el < end)
                            {
                                el++;
                                ret.Add(el);
                            }
                        else if (el > end)
                            while (el > end)
                            {
                                el--;
                                ret.Add(el);
                            }
                        index += 2;
                        break;
                    }
                }
                index++;
                return new Term(ret, (Type)type);
            }
            else if (curToken.StartsWith("\""))
            {
                var str = curToken;
                while(!curToken.EndsWith("\""))
                {
                    index++;
                    
                    if (tokens.Length == index + 1)
                        throw new Exception("no terminate string");

                    str += " " + curToken;
                }
                return Term.Parse(str, isExec);
            }
            return Term.Parse(curToken, isExec);
        }

        object doOp(object a, string code, object b)
        {
            switch (code)
            {
                case "*" : return (float)a *  (float)b;
                case "/" : return (float)a /  (float)b;
                case "%" : return (float)a %  (float)b;
                case "+" : return (float)a +  (float)b;
                case "-" : return (float)a -  (float)b;
                case ">" : return (float)a >  (float)b;
                case ">=": return (float)a >= (float)b;
                case "<" : return (float)a <  (float)b;
                case "<=": return (float)a <= (float)b;
                case "==": return (float)a == (float)b;
                case "!=": return (float)a != (float)b;
                case "&&": return (bool)a  && (bool)b;
                case "||": return (bool)a  || (bool)b;
                case "++": return (string)a + (string)b;
                case "===": return (string)a == (string)b;
                case "!!": return ((Term)a).At((int)(float)b);
                default:
                    throw new Exception(code + " operation is not defined");
            }
        }

        object checkOp(object a, object code, object b)
        {
            switch ((string)code)
            {
                case "*": 
                case "/": 
                case "%": 
                case "+": 
                case "-": 
                case ">": 
                case ">=":
                case "<": 
                case "<=":
                case "==":
                case "!=":
                    if ((Type)a == typeof(float) && (Type)b == typeof(float))
                        return typeof(float);
                    break;
                case "&&":
                case "||":
                    if ((Type)a == typeof(bool) && (Type)b == typeof(bool))
                        return typeof(bool);
                    break;
                case "++": 
                case "===":
                    if ((Type)a == typeof(string) && (Type)b == typeof(string))
                        return typeof(string);
                    break;
                case "!!":
                    if ((Type)b == typeof(float))
                        return (Type)a;
                    break;
                default:
                    throw new Exception(code + " operation is not defined");
            }
            throw new Exception(a + ", " + b + ", Type is mismatch");
        }
    }

    class FuncException : Exception
    {
        public int line;

        public FuncException(Exception e, int line) : base(e.Message, e)
        {
            this.line = line;
        }
    }

    class Term
    {
        static Dictionary<string, Term> vars;
        static Dictionary<string, Term> local;
        static Stack<Dictionary<string, Term>> locals;
        object content;
        public Type type;
        public FuncInfo info;
        public List<object> args;
        public static Term nil = new Term("", null);

        public Term(string word, FuncInfo info)
        {
            this.info = info;
            this.type = typeof(ComeFunc);
            this.content = (object)word;
            this.args = new List<object>();
        }

        public Term(object content, Type type)
        {
            this.content = content;
            this.type = type;
        }

        static Term toTerm(object content, Type type)
        {
            if (type == typeof(Term))
            {
                type = ((Term)content).type;
            }
            else if (type == typeof(ComeFunc))
            {
                return (Term)content;
            }
            return new Term(content, type);
        }

        public static void InitVars()
        {
            var gfuncs = new Dictionary<string, Term>();
            ComeFunc.List.ForEach(x => gfuncs.Add(x, new Term(x, new FuncInfo(x))));
            vars = gfuncs;

            local =  new Dictionary<string, Term>();
            locals = new Stack<Dictionary<string, Term>>();
        }

        public static object Parse(string word, bool isExec)
        {
            object content;
            Type type;
            var minus = false;

            if (word[0] == '-' && Char.IsLetter(word[1]))
            {
                word = word.Substring(1);
                minus = true;
            }

            float val;
            if (float.TryParse(word, out val))
            {
                content = val;
                type = typeof(float);
            }
            else if (word == "true" || word == "false")
            {
                content = word == "true";
                type = typeof(bool);
            }
            else if (word[0] == '"' && word[word.Length - 1] == '"')
            {
                content = word.Substring(1, word.Length - 2);
                type = typeof(string);
            }
            else if (vars.ContainsKey(word))
            {
                content = parseVars(vars[word], isExec, out type);
                if (minus)
                    content = -(float)content;
            }
            else if (local.ContainsKey(word))
            {
                content = parseVars(local[word], isExec, out type);
                if (minus)
                    content = -(float)content;
            }
            else
            {
                throw new Exception(word + " Term is not defined");
            }

            if (isExec)
                return content;
            else
            {
                if (type == typeof(ComeFunc))
                    return content;
                return type;
            }
        }

        static object parseVars(Term term, bool isExec, out Type type)
        {
            object content;

            if (term.info == null)
            {
                content = term.content;
                type = term.type;
            }
            else if (term.info.ArgCount == 1)
            {
                content = term.Eval(isExec);
                type = (Type)term.Eval(false);
            }
            else
            {
                var con = (Term)term.MemberwiseClone();
                con.args = new List<object>();
                foreach (var arg in term.args)
                    con.args.Add(arg);
                content = con;
                type = typeof(ComeFunc);
            }
            return content;
        }

        public static bool App(ref object func, object arg, bool isExec)
        {
            var f = (Term)func;
            f.args.Add(arg);

            if (f.args.Count == f.info.ArgCount - 1)
            {
                if (isExec)
                    func = ComeFunc.Eval(f);
                else
                    func = ComeFunc.Check(f);
                return true;
            }
            else
            {
                func = f;
                return false;
            }
        }

        public object Eval(bool isExec)
        {
            if (isExec)
                return ComeFunc.Eval(this);
            else
                return ComeFunc.Check(this);
        }

        public static void SettingLocal(FuncInfo info, List<object> args)
        {
            var names = info.argnames;
            var types = info.argtypes;
            if (names.Length != types.Length - 1 || names.Length != args.Count)
                throw new Exception("function's paramator is incorrect");

            locals.Push(local);
            local = new Dictionary<string, Term>();
            for (int i = 0; i < names.Length; i++)
            {
                local.Add(names[i], new Term(args[i], types[i]));
            }
        }

        public static void ReleaseLocal()
        {
            local = locals.Pop();
        }

        public static void RegistVar(string name, object content, Type type)
        {
            var term = toTerm(content, type);
            if (locals.Count == 0)
                vars[name] = term;
            else
                local[name] = term;
        }

        public static void SetFuncEnd(string name, int line)
        {
            var func = (Term)vars[name];
            func.info.end = line;
        }

        public static void Assign(string name, object content, Type type)
        {
            var term = toTerm(content, type);
            if (vars.ContainsKey(name))
            {
                if (vars[name].type == term.type)
                    vars[name] = term;
                else
                    throw new Exception(vars[name].type + ", " + term.type + ": type is mismatch");
            }
            else if (local.ContainsKey(name))
                if (local[name].type == term.type)
                    local[name] = term;
                else
                    throw new Exception(local[name].type + ", " + term.type + ": type is mismatch");
            else
                throw new Exception("no variable: " + name);
        }


        public object At(int atIx)
        {
            var list = ((List<object>)content);
            if (atIx > list.Count - 1 || atIx < 0)
                throw new Exception(atIx + ", index is out of size: " + list.Count);
            return list[atIx];
        }

        public string Str { get { return (string)content; } }

        public List<object> List { get { return (List<object>)content; } }

        public int Length { get { return ((List<object>)content).Count; } }

        public Term Clone()
        {
            var copy = (Term)MemberwiseClone();
            copy.content = new List<object>((List<object>)copy.content);
            return copy;
        }
    }

    class ComeFunc
    {
        static ComeLang comelang;
        static object result;
        public static List<string> List;

        public static object Eval(Term func)
        {
            var a = func.args;
            switch (func.Str)
            {
                case "setbrush": FuncDef.setbrush((string)a[0]); break;
                case "fillrect": FuncDef.fillrect((float)a[0], (float)a[1], (float)a[2], (float)a[3]); break;
                case "print": FuncDef.print((string)a[0], (float)a[1], (float)a[2]); break;
                case "invalidate": FuncDef.invalidate(); break;
                case "random": return FuncDef.random((float)a[0]);
                case "str": return FuncDef.str(a[0]);
                case "input": return FuncDef.input();
                case "untilinput": return FuncDef.untilinput();
                case "clone": return FuncDef.clone((Term)a[0]);
                default:
                    if (func.info != null)
                        return comelang.DefineFunc(func);
                    else
                        throw new Exception(func.Str + " is not a function");
            }
            return null;
        }

        public static Type Check(Term func)
        {
            func.args.Zip(func.info.argtypes, (x, type) =>
            {
                if (type == typeof(Term))
                    return true;
                var t = x.GetType();
                if (t.Name == "RuntimeType")
                    t = (Type)x;
                if (t != type)
                    throw new Exception(t + " : " + " is not match function argment type of: " + type);
                else
                    return true;
            }
            ).ToList();

            return func.info.argtypes.Last();
        }

        public static void Init(Graphics grfx, ComeLang lang, ViewForm form)
        {
            ComeFunc.comelang = lang;
            FuncDef.grfx = grfx;
            FuncDef.comelang = lang;
            FuncDef.form = form;
            ComeFunc.List = typeof(FuncDef).GetMethods().Select(x=>x.Name).ToList();
        }

        public static object Result
        {
            get
            {
                var ret = result;
                result = null;
                return ret;
            }
            set
            {
                if (result == null)
                    result = value;
                else
                    throw new Exception("function return value is not used");
            }
        }

        public static Type[] ArgTypes(string func)
        {
            var method = typeof(FuncDef).GetMethod(func);
            var pars = method.GetParameters();
            var partypes = pars.Select(x=>x.ParameterType).ToList();
            partypes.Add(method.ReturnType);
            return partypes.ToArray();
        }

        public static String[] ArgNames(string func)
        {
            var method = typeof(FuncDef).GetMethod(func);
            var pars = method.GetParameters();
            var parnames = pars.Select(x=>x.Name).ToArray();
            return parnames;
        }

        /*
        
        Dictionary<string, Func<ComeFunc, Term[], Term>> cache =
            new Dictionary<string, Func<ComeFunc, Term[], Term>>();

        
        public Term Eval(string func, Term[] args)
        {
            if (func == "defines")
                return comelang.DefineFunc(args);

            if (!cache.ContainsKey(func))
            {
                var method = typeof(ComeFunc).GetMethod(func);
                var methodDelegate = (Func<ComeFunc, Term[], Term>)Delegate
                    .CreateDelegate(typeof(Func<ComeFunc, Term[], Term>), method);
                cache.Add(func, methodDelegate);
            }
            return cache[func](this, args);
        }
         
        */
    }

    class FuncDef
    {
        public static Graphics grfx;
        public static ComeLang comelang;
        public static ViewForm form;
        static Font midmsfont = new Font("MS UI Gothic", 12);
        static Font bigmsfont = new Font("MS UI Gothic", 24);
        static Font curfont = midmsfont;
        static Brush curbrush = Brushes.Black;
        static Random rand = new Random();

        public static void setbrush(string color)
        {
            curbrush = new SolidBrush(Color.FromName(color));
        }

        public static void fillrect(float w, float h, float x , float y)
        {
            grfx.FillRectangle(curbrush, x, y, w, h);
        }

        public static void print(string str, float x, float y)
        {
            grfx.DrawString(str, curfont, curbrush, x, y);
        }

        public static void invalidate() { form.Invalidate(); }

        public static float random(float max) { return rand.Next((int)max); }

        public static string str(object obj) { return obj.ToString(); }

        public static string input()
        {
            if (comelang.InQueue.Count > 0)
                return comelang.InQueue.Dequeue();
            return "";
        }

        public static string untilinput()
        {
            while (comelang.InQueue.Count == 0)
                System.Threading.Thread.Sleep(500);

            return comelang.InQueue.Dequeue();
        }

        public static Term clone(Term term) { return term.Clone(); }

    }

    class FuncInfo
    {
        public int begin;
        public int end;
        public string[] argnames;
        public Type[] argtypes;

        public FuncInfo() { }

        public FuncInfo(int begin, int end, string[] argnames, Type[] argtypes)
        {
            this.begin = begin;
            this.end = end;
            this.argnames = argnames;
            this.argtypes = argtypes;
        }

        public FuncInfo(string func)
        {
            this.begin = 0;
            this.end = 0;
            this.argnames = ComeFunc.ArgNames(func);
            this.argtypes = ComeFunc.ArgTypes(func);
        }

        public Type ArgType(int num)
        {
            return argtypes[num];
        }

        public int ArgCount
        {
            get { return argtypes.Length; }
        }
    }
}
