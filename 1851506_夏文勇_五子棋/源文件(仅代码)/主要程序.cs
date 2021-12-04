using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;



public struct Point
{
    public int Zvalue;
    public int deep;
    public Point(int _Zvalue, int _deep)
    {
        Zvalue = _Zvalue;
        deep = _deep;
    }
};
namespace gobang
{
    public partial class Form1 : Form
    {
        private bool start = false;     //现在的棋局状态，是否开始
        private bool Ai = false;        //是否人机模式
        private bool AiUse = false;     //电脑在下棋，不能下
        private bool who = true;        //谁下棋0黑，1白

        private int[,] chessNum = new int[Global.size, Global.size];    //每一步是第几步下的

        private Dictionary<int, bool> mp = new Dictionary<int, bool>();  //Zobrist随机值判重

        private int prex = -1, prey = -1;         //上一步下的棋的位置

        private int[,] point = new int[230, 2];//下棋的记录
        private int totSteps = 0;


        public Form1()
        {
            InitializeComponent();

        }

        //Zobrist随机值，每个落点3个状态
        private void Zobrist()
        {
            Random ran = new Random();
            int RandKey;
            for (int i = 0; i < Global.size; i++)
            {
                for (int j = 0; j < Global.size; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        RandKey = ran.Next();
                        while (mp.ContainsKey(RandKey))
                        {
                            RandKey = ran.Next();
                        }
                        Global.ZobristMap[i, j, k] = RandKey;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "黑色方下棋";
            start = true;
            Ai = false;
            button1.Enabled = false;
            buttonWhite.Enabled = false;
            buttonBlack.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            init();
        }

        private void init()
        {
            label1.Text = "游戏尚未开始！";
            start = false;
            AiUse = false;
            button1.Enabled = true;
            buttonWhite.Enabled = true;
            buttonBlack.Enabled = true;
            button2.Enabled = false;
            for (int i = 0; i < Global.size; i++)
                for (int j = 0; j < Global.size; j++)
                    Global.chessmap[i, j] = Global.CHESS_NONE;
            who = true;     //黑棋先下
            chessboard.nowLeft = Global.size;
            chessboard.nowRight = 0;
            chessboard.nowUp = Global.size;
            chessboard.nowDown = 0;
            prex = -1;
            prey = -1;
            totSteps = 0;

            

            AI_solve.init();
            Graphics graph = panel2.CreateGraphics();
            chessboard.draw_chessboard(graph);//重新加载（画）棋盘
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Zobrist();
            init();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics graph = panel2.CreateGraphics();
            chessboard.draw_chessboard(graph);//重新加载（画）棋盘
            //Chess.ReDrawC(panel1, Global.chessmap);//重新加载（画）棋子。
        }


        private void moveNow(object sender, ref bool who, int x1, int y1)
        {
            //下黑子还是白子
            if (who == true)
            {
                Global.chessmap[x1, y1] = Global.CHESS_BLACK;
                Global.ZovristHash ^= Global.ZobristMap[x1, y1, Global.CHESS_BLACK];
                Global.ZovristHash ^= Global.ZobristMap[x1, y1, Global.CHESS_NONE];
            }
            else
            {
                Global.chessmap[x1, y1] = Global.CHESS_WHITE;
                Global.ZovristHash ^= Global.ZobristMap[x1, y1, Global.CHESS_WHITE];
                Global.ZovristHash ^= Global.ZobristMap[x1, y1, Global.CHESS_NONE];
            }
            point[totSteps, 0] = x1;
            point[totSteps, 1] = y1;
            totSteps++;
            chessNum[x1, y1] = totSteps;
            chessboard.nowLeft = (x1 - AI_solve.changeRange < chessboard.nowLeft) ? x1 - AI_solve.changeRange : chessboard.nowLeft;
            chessboard.nowRight = (x1 + AI_solve.changeRange > chessboard.nowRight) ? x1 + AI_solve.changeRange : chessboard.nowRight;
            chessboard.nowUp = (y1 - AI_solve.changeRange < chessboard.nowUp) ? y1 - AI_solve.changeRange : chessboard.nowUp;
            chessboard.nowDown = (y1 + AI_solve.changeRange > chessboard.nowDown) ? y1 + AI_solve.changeRange : chessboard.nowDown;
            if (chessboard.nowLeft < 0) chessboard.nowLeft = 0;
            if (chessboard.nowRight > Global.size) chessboard.nowRight = Global.size;
            if (chessboard.nowUp < 0) chessboard.nowUp = 0;
            if (chessboard.nowDown > Global.size) chessboard.nowDown = Global.size;


            //画棋子
            if (prex != -1)
                chessboard.draw_chess(panel2, !who, prex, prey, totSteps - 1);
            chessboard.draw_chess(panel2, who, x1, y1, totSteps, true);
            prex = x1;
            prey = y1;

            //判断是否胜利
            if (chessboard.isVictory(x1, y1))
            {
                if (who == true)
                    MessageBox.Show("黑方胜利(Black Win)");
                else
                    MessageBox.Show("白方胜利(White Win)");
                init();
                return;
            }
            else if (chessboard.mapFull())
            {
                MessageBox.Show("平局");
                init();
                return;
            }

            who = !who;
            if (who == true)
                label1.Text = "黑色方下棋";
            else
                label1.Text = "白色方下棋";
        }


        //根据鼠标点击的位置画棋子
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            //把棋盘分为【15，15】的数组
            if (start && !AiUse )
            {
                int x1 = (e.X - chessboard.Margin + chessboard.pointGap / 2) / chessboard.pointGap;
                int y1 = (e.Y - chessboard.Margin + chessboard.pointGap / 2) / chessboard.pointGap;
                int xnext = 0, ynext = 0;

                try
                {
                    //判断此位置是否为空
                    if (x1 < 0 || y1 < 0 || x1 >= Global.size || y1 >= Global.size || Global.chessmap[x1, y1] != Global.CHESS_NONE)
                    {
                        return;//已经有棋子占领这个位置了
                    }

                    moveNow(sender, ref who, x1, y1);

                    if (start == false)
                        return;

                    // label2.Text = (AI_solve.judge(  who, x1, y1) * 4 + AI_solve.judge(  !who, x1, y1)).ToString();

                    //AI_solve.calV(  who);
                    //AI_solve.calV(  !who);
                    if (Ai)
                    {
                        AiUse = true;
                        button2.Enabled = false;
                        button3.Enabled = false;
                        button4.Enabled = false;
                        label2.Text = AI_solve.getAns(who, ref xnext, ref ynext).ToString();
                        moveNow(sender, ref who, xnext, ynext);
                        button2.Enabled = true;
                        button3.Enabled = true;
                        button4.Enabled = true;
                        AiUse = false;
                    }
                }
                catch (Exception)
                {
                    //防止因鼠标点击边界，而导致数组越界，进而运行中断。
                    for (int i = 0; i < Global.size; i++)
                    {
                        for (int j = 0; j < Global.size; j++)
                        {
                            if (Global.chessmap[i, j] != Global.CHESS_NONE)
                            {
                                chessboard.draw_chess(panel2, Global.chessmap[i, j] == Global.CHESS_BLACK, i, j, chessNum[i, j]);
                            }
                        }
                    }
                }
            }
            else
            {
                if(!start)
                    MessageBox.Show("请先开始游戏！", "提示信息！", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonBlack_Click(object sender, EventArgs e)
        {
            label1.Text = "黑色方下棋";
            start = true;
            Ai = true;
            button1.Enabled = false;
            buttonWhite.Enabled = false;
            buttonBlack.Enabled = false;
            button2.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (totSteps == 0)
            {
                MessageBox.Show("未落子！");
                return;
            }

            if (Ai)
            {
                Global.ZovristHash ^= Global.ZobristMap[point[totSteps - 1, 0], point[totSteps - 1, 1], Global.chessmap[point[totSteps - 1, 0], point[totSteps - 1, 1]]];
                Global.ZovristHash ^= Global.ZobristMap[point[totSteps - 1, 0], point[totSteps - 1, 1], Global.CHESS_NONE];
                Global.ZovristHash ^= Global.ZobristMap[point[totSteps - 2, 0], point[totSteps - 2, 1], Global.chessmap[point[totSteps - 2, 0], point[totSteps - 2, 1]]];
                Global.ZovristHash ^= Global.ZobristMap[point[totSteps - 2, 0], point[totSteps - 2, 1], Global.CHESS_NONE];
                Global.chessmap[point[totSteps - 1, 0], point[totSteps - 1, 1]] = Global.CHESS_NONE;
                Global.chessmap[point[totSteps - 2, 0], point[totSteps - 2, 1]] = Global.CHESS_NONE;
                totSteps -= 2;
                Graphics graph = panel2.CreateGraphics();
                chessboard.draw_chessboard(graph);//重新加载（画）棋盘
                for (int i = 0; i < Global.size; i++)
                {
                    for (int j = 0; j < Global.size; j++)
                    {
                        if (Global.chessmap[i, j] != Global.CHESS_NONE)
                        {
                            if (i == point[totSteps - 1, 0] && j == point[totSteps - 1, 1])
                                chessboard.draw_chess(panel2, Global.chessmap[i, j] == Global.CHESS_BLACK, i, j, chessNum[i, j], true);
                            else
                                chessboard.draw_chess(panel2, Global.chessmap[i, j] == Global.CHESS_BLACK, i, j, chessNum[i, j]);
                        }
                    }
                }
            }
            else
            {
                Global.ZovristHash ^= Global.ZobristMap[point[totSteps - 1, 0], point[totSteps - 1, 1], Global.chessmap[point[totSteps - 1, 0], point[totSteps - 1, 1]]];
                Global.ZovristHash ^= Global.ZobristMap[point[totSteps - 1, 0], point[totSteps - 1, 1], Global.CHESS_NONE];
                Global.chessmap[point[totSteps - 1, 0], point[totSteps - 1, 1]] = Global.CHESS_NONE;
                totSteps -= 1;
                Graphics graph = panel2.CreateGraphics();
                chessboard.draw_chessboard(graph);//重新加载（画）棋盘
                for (int i = 0; i < Global.size; i++)
                {
                    for (int j = 0; j < Global.size; j++)
                    {
                        if (Global.chessmap[i, j] != Global.CHESS_NONE)
                        {
                            if (i == point[totSteps - 1, 0] && j == point[totSteps - 1, 1])
                                chessboard.draw_chess(panel2, Global.chessmap[i, j] == Global.CHESS_BLACK, i, j, chessNum[i, j], true);
                            else
                                chessboard.draw_chess(panel2, Global.chessmap[i, j] == Global.CHESS_BLACK, i, j, chessNum[i, j]);
                        }
                    }
                }
                who = !who;
            }
            prex = -1;
            prey = -1;
            if (totSteps > 0)
            {
                prex = point[totSteps - 1, 0];
                prey = point[totSteps - 1, 1];
            }
        }

        private void buttonWhite_Click(object sender, EventArgs e)
        {
            label1.Text = "黑色方下棋";
            start = true;
            Ai = true;
            button1.Enabled = false;
            buttonWhite.Enabled = false;
            buttonBlack.Enabled = false;
            button2.Enabled = true;

            moveNow(sender, ref who, Global.size / 2, Global.size / 2);
        }
    }

    class Global
    {
        public const int CHESS_NONE = 0;
        public const int CHESS_BLACK = 1;
        public const int CHESS_WHITE = 2;
        public const int size = 15;    //棋盘大小
        public static int[,,] ZobristMap = new int[Global.size, Global.size, 3];    //Zobrist随机值
        public static int ZovristHash { get; set; }        //Zobrist哈希值
        public static Dictionary<Point, int> ReplacementTable = new Dictionary<Point, int>();  //Zobrist随机值判重

        public static int[,] chessmap = new int[size, size];    //棋盘状态，0空，1黑，2白

    };

    class chessboard
    {
        private static int[,] dir = new int[8, 2] { { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 } };
        //上，右上，右，右下，下，左下，左，左上
        private static int[] cntCon = new int[8];
        public static int nowLeft, nowRight, nowUp, nowDown;//现在已经下的子的边界
        public const int pointGap = 32, Margin = 40;   //画布大小,行距，边界距离
        public const int chessRadius = 24;


        public static bool mapFull()
        {
            for (int i = 0; i < Global.size; i++)
            {
                for (int j = 0; j < Global.size; j++)
                {
                    if (Global.chessmap[i, j] != Global.CHESS_NONE)
                        return false;
                }
            }
            return true;
        }

        public static bool isVictory(int x, int y)
        {
            int xnew, ynew;
            for (int i = 0; i < 8; i++)
            {
                xnew = x;
                ynew = y;
                cntCon[i] = 0;
                for (int j = 0; j <= 5; j++)
                {
                    xnew += dir[i, 0];
                    ynew += dir[i, 1];
                    if (xnew < 0 || xnew >= Global.size || ynew < 0 || ynew >= Global.size || Global.chessmap[xnew, ynew] != Global.chessmap[x, y])
                        break;
                    cntCon[i]++;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (cntCon[i] + cntCon[i + 4] >= 4)
                {
                    return true;
                }
            }
            return false;
        }



        //找第i个数的坐标的值
        public static int count_position(int i)
        {
            return Margin + pointGap * (i - 1);
        }

        public static void draw_chessboard(Graphics graph)
        {
            graph.Clear(Color.LightSkyBlue);
            for (int i = 1; i <= Global.size; i++)
            {
                graph.DrawLine(new Pen(Color.Black), Margin, count_position(i), count_position(Global.size), count_position(i));
                graph.DrawLine(new Pen(Color.Black), count_position(i), Margin, count_position(i), count_position(Global.size));
            }
            graph.FillEllipse(new SolidBrush(Color.Black), count_position(8)- chessRadius / 6, count_position(8)- chessRadius / 6, chessRadius/3, chessRadius/3);
            graph.FillEllipse(new SolidBrush(Color.Black), count_position(4) - chessRadius / 6, count_position(4) - chessRadius / 6, chessRadius / 3, chessRadius / 3);
            graph.FillEllipse(new SolidBrush(Color.Black), count_position(4) - chessRadius / 6, count_position(12) - chessRadius / 6, chessRadius / 3, chessRadius / 3);
            graph.FillEllipse(new SolidBrush(Color.Black), count_position(12) - chessRadius / 6, count_position(4) - chessRadius / 6, chessRadius / 3, chessRadius / 3);
            graph.FillEllipse(new SolidBrush(Color.Black), count_position(12) - chessRadius / 6, count_position(12) - chessRadius / 6, chessRadius / 3, chessRadius / 3);
        }
        public static void draw_chess(Panel p, bool who, int x1, int y1, int num, bool red = false)
        {
            Graphics g = p.CreateGraphics();
            //确定棋子的中心位置
            int x2 = x1 * pointGap + Margin - chessRadius / 2;
            int y2 = y1 * pointGap + Margin - chessRadius / 2;

            if (red)
            {
                g.DrawLine(new Pen(Color.Red, 2), x2 - chessRadius / 8, y2 - chessRadius / 8, x2 - chessRadius / 8, y2 + chessRadius / 8);
                g.DrawLine(new Pen(Color.Red, 2), x2 - chessRadius / 8, y2 - chessRadius / 8, x2 + chessRadius / 8, y2 - chessRadius / 8);
                g.DrawLine(new Pen(Color.Red, 2), x2 + chessRadius / 8 * 9, y2 + chessRadius / 8 * 9, x2 + chessRadius / 8 * 7, y2 + chessRadius / 8 * 9);
                g.DrawLine(new Pen(Color.Red, 2), x2 + chessRadius / 8 * 9, y2 + chessRadius / 8 * 9, x2 + chessRadius / 8 * 9, y2 + chessRadius / 8 * 7);
                g.DrawLine(new Pen(Color.Red, 2), x2 + chessRadius / 8 * 9, y2 - chessRadius / 8, x2 + chessRadius / 8 * 7, y2 - chessRadius / 8);
                g.DrawLine(new Pen(Color.Red, 2), x2 + chessRadius / 8 * 9, y2 - chessRadius / 8, x2 + chessRadius / 8 * 9, y2 + chessRadius / 8);
                g.DrawLine(new Pen(Color.Red, 2), x2 - chessRadius / 8, y2 + chessRadius / 8 * 9, x2 - chessRadius / 8, y2 + chessRadius / 8 * 7);
                g.DrawLine(new Pen(Color.Red, 2), x2 - chessRadius / 8, y2 + chessRadius / 8 * 9, x2 + chessRadius / 8, y2 + chessRadius / 8 * 9);
                //g.FillEllipse(new SolidBrush(Color.Red), x2 + chessRadius / 4, y2 + chessRadius / 4, chessRadius / 2, chessRadius / 2);
            }
            else
            {
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 - chessRadius / 8, y2 - chessRadius / 8, x2 - chessRadius / 8, y2 + chessRadius / 8);
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 - chessRadius / 8, y2 - chessRadius / 8, x2 + chessRadius / 8, y2 - chessRadius / 8);
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 + chessRadius / 8 * 9, y2 + chessRadius / 8 * 9, x2 + chessRadius / 8 * 7, y2 + chessRadius / 8 * 9);
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 + chessRadius / 8 * 9, y2 + chessRadius / 8 * 9, x2 + chessRadius / 8 * 9, y2 + chessRadius / 8 * 7);
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 + chessRadius / 8 * 9, y2 - chessRadius / 8, x2 + chessRadius / 8 * 7, y2 - chessRadius / 8);
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 + chessRadius / 8 * 9, y2 - chessRadius / 8, x2 + chessRadius / 8 * 9, y2 + chessRadius / 8);
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 - chessRadius / 8, y2 + chessRadius / 8 * 9, x2 - chessRadius / 8, y2 + chessRadius / 8 * 7);
                g.DrawLine(new Pen(Color.LightSkyBlue, 2), x2 - chessRadius / 8, y2 + chessRadius / 8 * 9, x2 + chessRadius / 8, y2 + chessRadius / 8 * 9);
            }

            Font font = new Font("楷体", 10, FontStyle.Regular);//设置字体

            if (who)
            {
                g.FillEllipse(new SolidBrush(Color.Black), x2, y2, chessRadius, chessRadius);
                if (num < 10)
                    g.DrawString(num.ToString(), font, Brushes.White, x2 + 7, y2 + chessRadius / 4);//Brushes.:字的颜色
                else if (num < 100)
                    g.DrawString(num.ToString(), font, Brushes.White, x2 + chessRadius / 7, y2 + chessRadius / 4);//Brushes.:字的颜色
                else
                    g.DrawString(num.ToString(), font, Brushes.White, x2, y2 + chessRadius / 4);//Brushes.:字的颜色
            }
            else
            {
                g.FillEllipse(new SolidBrush(Color.White), x2, y2, chessRadius, chessRadius);
                if (num < 10)
                    g.DrawString(num.ToString(), font, Brushes.Black, x2 + 7, y2 + chessRadius / 4);//Brushes.:字的颜色
                else if (num < 100)
                    g.DrawString(num.ToString(), font, Brushes.Black, x2 + chessRadius / 7, y2 + chessRadius / 4);//Brushes.:字的颜色
                else
                    g.DrawString(num.ToString(), font, Brushes.Black, x2, y2 + chessRadius / 4);//Brushes.:字的颜色
            }
        }
    }

    class AI_solve
    {
        private static int[,] dir = new int[8, 2] { { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 } };
        //左，左下，下，右下，右，右上，上，左上
        private static int[] cntCon = new int[8];      //连续的相同棋子或空白
        private static int[,] cntNum = new int[8, 5];  //向一个方向延伸，遇到不同颜色棋子或指定长度前，相同棋子数量
        private static int[] cntChong = new int[6];
        private static int[] cntLive = new int[6];
        private static int cntDie;

        public const int changeRange = 4;      //每个节点能够影响的范围
        private const int maxDeep = 4;          //搜索最大深度
        private const int maxSearchPoint = 10;  //每层搜索节点数量

        public static int[,,] Vmap = new int[2, Global.size, Global.size];        //记录每个节点对于两种颜色的权值，1黑0白
        private static bool[,] maxVmapVis = new bool[Global.size, Global.size];     //Max剪枝时取前十个最值
        private static int xnext, ynext;

        public static void init()
        {
            Global.ZovristHash = 0;
            for (int i = 0; i < Global.size; i++)
            {
                for (int j = 0; j < Global.size; j++)
                {
                    Global.ZovristHash ^= Global.ZobristMap[i, j, Global.CHESS_NONE];
                }
            }

            xnext = -1;
            ynext = -1;
            for (int now = 0; now <= 1; now++)
            {
                for (int i = 0; i < Global.size; i++)
                {
                    for (int j = 0; j < Global.size; j++)
                    {
                        Vmap[now, i, j] = 0;
                    }
                }
            }
        }


        //更新改动的方格周围(changeRange*2+1)^2个节点
        private static void changeVmap(int x, int y)
        {
            for (int i = algorithm.max(0, x - changeRange); i < algorithm.min(Global.size, x + changeRange); i++)
            {
                for (int j = algorithm.max(0, y - changeRange); j < algorithm.min(Global.size, y + changeRange); j++)
                {
                    if (Global.chessmap[i, j] != Global.CHESS_NONE)
                    {
                        Vmap[0, i, j] = 0;
                        Vmap[1, i, j] = 0;
                        continue;
                    }
                    Vmap[1, i, j] = judge(true, i, j);
                    Vmap[0, i, j] = judge(false, i, j);
                }
            }
        }


        //deep偶数Max,奇数Min
        private static int alpha_beta(int deep, bool who, int x, int y, int alpha, int beta)
        {
            Point p1 = new Point(Global.ZovristHash, deep);
            if(deep!=maxDeep && Global.ReplacementTable.ContainsKey(p1))
            {
                return Global.ReplacementTable[p1];
            }

            int usenow = who ? 1 : 0;
            if (x != -1)
            {
                Point pnow = new Point(Global.ZovristHash, deep);

                Global.chessmap[x, y] = (2 - usenow);
                Global.ZovristHash ^= Global.ZobristMap[x, y, Global.chessmap[x, y]];
                Global.ZovristHash ^= Global.ZobristMap[x, y, Global.CHESS_NONE];
            }


            if (deep <= 1)
            {
                Vmap[1, x, y] = judge(true, x, y);
                Vmap[0, x, y] = judge(false, x, y);
                Global.ZovristHash ^= Global.ZobristMap[x, y, Global.chessmap[x, y]];
                Global.ZovristHash ^= Global.ZobristMap[x, y, Global.CHESS_NONE];
                Global.chessmap[x, y] = Global.CHESS_NONE;
                return calV_real(usenow, x, y);
                //return ansnow;
            }

            p1.Zvalue = Global.ZovristHash;
            //更新改动的方格周围(changeRange*2+1)^2个节点
            if (x != -1)
                changeVmap(x, y);


            int ansnow;
            bool flag = true;
            int Vnow_ = 0;
            if (x != -1)
                Vnow_ = calV_real(usenow, x, y);

            if (deep % 2 == 0)//Max
            {
                if (x != -1 && Vnow_ > alpha)
                {
                    if (deep == maxDeep)
                    {
                        xnext = x;
                        ynext = y;
                    }
                    alpha = Vnow_;
                }

                for (int i = chessboard.nowLeft; i < chessboard.nowRight; i++)
                    for (int j = chessboard.nowUp; j < chessboard.nowDown; j++)
                        maxVmapVis[i, j] = false;

                for (int num = 0; num < maxSearchPoint; num++)
                {
                    int maxnow = -1000000, xnow = 0, ynow = 0;
                    for (int i = chessboard.nowLeft; i < chessboard.nowRight; i++)
                    {
                        for (int j = chessboard.nowUp; j < chessboard.nowDown; j++)
                        {
                            if (maxVmapVis[i, j] || Global.chessmap[i, j] != Global.CHESS_NONE)
                                continue;
                            if (calV_real(usenow, i, j) > maxnow)
                            {
                                maxnow = calV_real(usenow, i, j);
                                xnow = i;
                                ynow = j;
                            }
                        }
                    }
                    maxVmapVis[xnow, ynow] = true;
                    ansnow = alpha_beta(deep - 1, !who, xnow, ynow, alpha, beta);

                    if (ansnow > alpha)
                    {
                        if (deep == maxDeep)
                        {
                            xnext = xnow;
                            ynext = ynow;
                        }
                        alpha = ansnow;
                    }
                    if (alpha >= beta)
                        break;
                }
            }
            else            //Min
            {
                for (int i = chessboard.nowLeft; i < chessboard.nowRight && flag; i++)
                {
                    for (int j = chessboard.nowUp; j < chessboard.nowDown && flag; j++)
                    {
                        if (Global.chessmap[i, j] == Global.CHESS_NONE)
                        {
                            //Global.chessmap[i, j] = (2 - usenow);
                            ansnow = alpha_beta(deep - 1, !who, i, j, alpha, beta);
                            if (ansnow < beta)
                                beta = ansnow;
                            if (alpha >= beta)
                                flag = false;
                        }
                    }
                }
            }

            if (x != -1)
            {
                Global.ZovristHash ^= Global.ZobristMap[x, y, Global.chessmap[x, y]];
                Global.ZovristHash ^= Global.ZobristMap[x, y, Global.CHESS_NONE];
                Global.chessmap[x, y] = Global.CHESS_NONE;
            }

            //恢复改动的方格周围(changeRange*2+1)^2个节点
            if (x != -1)
                changeVmap(x, y);

            if (deep % 2 == 0)   //Max
            {
                Global.ReplacementTable[p1] = (int)(alpha * 0.9);
                return (int)(alpha * 0.9);
            }
            else                //Min
            {
                Global.ReplacementTable[p1] = (int)(beta * 0.9);
                return (int)(beta * 0.9);
            }
        }


        private static int calV_real(int now, int x, int y)
        {
            return Vmap[now, x, y] * 2 + Vmap[1 - now, x, y];
        }

        public static int getAns(bool who, ref int x, ref int y)
        {
            xnext = 0;
            ynext = 0;
            calV(who);
            calV(!who);
            int now = who ? 1 : 0;
            int maxx = 0, ansnow = 0;
            x = 0;
            y = 0;
            for (int i = 0; i < Global.size; i++)
            {
                for (int j = 0; j < Global.size; j++)
                {
                    ansnow = calV_real(now, i, j);
                    if (ansnow > maxx)
                    {
                        maxx = ansnow;
                        x = i;
                        y = j;
                    }
                }
            }
            if (maxx >= 10000) //近程有必下的棋
            {
                return maxx;
            }


            //print(who);

            ansnow = alpha_beta(maxDeep, who, -1, -1, -100000000, 100000000);
            x = xnext;
            y = ynext;
            return ansnow;
        }

        private static void print(bool who)
        {
            using (StreamWriter sw = new StreamWriter("D://names.txt", append: true))
            {
                for (int now = 0; now <= 1; now++)
                {
                    for (int i = 0; i < Global.size; i++)
                    {
                        for (int j = 0; j < Global.size; j++)
                        {
                            sw.Write(Vmap[now, i, j]);
                            sw.Write(" ");
                        }
                        sw.Write("\n");
                    }
                    sw.Write("\n\n\n");
                }
            }
        }

        /**********************************************
         * 评估函数
         * 
         **********************************************/
        public static void calV(bool who)
        {
            int now = (who ? 1 : 0);
            for (int i = chessboard.nowLeft; i < chessboard.nowRight; i++)
            {
                for (int j = chessboard.nowUp; j < chessboard.nowDown; j++)
                {
                    if (Global.chessmap[i, j] != Global.CHESS_NONE)
                    {
                        Vmap[now, i, j] = 0;
                        continue;
                    }
                    Vmap[now, i, j] = judge(who, i, j);
                }
            }
        }


        public static int judge(bool who, int i, int j)
        {
            countConNum(who, i, j);
            for (int k = 0; k <= 5; k++)
            {
                cntChong[k] = 0;
                cntLive[k] = 0;
            }
            cntDie = 0;
            for (int fx = 0; fx < 4; fx++)
            {
                cntChong[statusChong(fx) + 1]++;
                cntLive[statusLive(fx) + 1]++;
                if (statusDie(fx) <= 3)
                    cntDie++;
            }
            int ans = 0;
            ans = algorithm.min(i, j, Global.size - i - 1, Global.size - j - 1);
            if (cntChong[5] > 0)
                ans = 3000000;
            else if (cntLive[4] > 0 || cntChong[4] > 1 || (cntChong[4] > 0 && cntLive[3] > 1))
                ans = 90000;
            else if (cntLive[3] > 1)
                ans = 12000;
            else if (cntLive[3] > 0 && cntChong[3] > 1)
                ans = 3000;
            else if (cntChong[4] > 0)
                ans = 1500;
            else if (cntLive[3] > 0)
                ans = 600;
            else if (cntLive[2] > 1)
                ans = 300;
            else if (cntChong[3] > 0)
                ans = 150;
            else if (cntChong[2] > 1 && cntLive[2] > 0)
                ans = 30;
            else if (cntLive[2] > 0)
                ans = 15;
            else if (cntChong[2] > 1)
                ans = 9;

            ans -= 5 * cntDie;
            return ans;
        }

        //计算位置[i,j]八个方向的连续值
        private static void countConNum(bool who, int i, int j)
        {
            int xnew, ynew;
            int numnow = 2; //现在下棋的颜色,默认白色
            if (who)
                numnow = 1;
            for (int fx = 0; fx < 8; fx++)
            {
                xnew = i;
                ynew = j;

                cntCon[fx] = 0;
                for (int k = 0; k < 4; k++)
                {
                    cntNum[fx, k] = 0;
                }

                for (int k = 0; k < 4; k++)
                {
                    xnew += dir[fx, 0];
                    ynew += dir[fx, 1];
                    //越界或数字不同，被阻隔
                    if (xnew < 0 || xnew >= Global.size || ynew < 0 || ynew >= Global.size || Global.chessmap[xnew, ynew] == (3 - numnow))
                        break;
                    cntNum[fx, k + 1] = cntNum[fx, k];

                    cntCon[fx]++;
                    if (Global.chessmap[xnew, ynew] == numnow)
                        cntNum[fx, k + 1]++;
                }
            }
        }


        /****************************
         * 冲的状态，连续五个：
         * 五个，获胜；
         * 四个，冲四；
         * 三个，冲三；
         * 两个，冲二；
         * *************************/
        private static int statusChong(int fx)
        {
            const int scope = 4;
            int num2, maxx = 0;
            for (int num = cntCon[fx]; num >= 0; num--)
            {
                num2 = scope - num;
                if (num2 > cntCon[fx + 4])
                    break;
                if (cntNum[fx, num] + cntNum[fx + 4, num2] > maxx)
                    maxx = cntNum[fx, num] + cntNum[fx + 4, num2];
            }
            return maxx;
        }

        /*****************************
        * 活的状态，一共六个，首尾必须为空，中间连续四个：
        * 四个，活四；
        * 三个，活三；
        * 两个，活二；
        ****************************/
        private static int statusLive(int fx)
        {
            const int scope = 3;
            int num2, maxx = 0;
            for (int num = cntCon[fx] - 1; num >= 0; num--)
            {
                num2 = scope - num;
                if (num2 >= cntCon[fx + 4])
                    break;
                if (cntNum[fx, num] + cntNum[fx + 4, num2] > maxx)
                    maxx = cntNum[fx, num] + cntNum[fx + 4, num2];
            }
            return maxx;
        }

        /*****************************
        * 死的状态，看最大连续
        ****************************/
        private static int statusDie(int fx)
        {
            return cntCon[fx] + cntCon[fx + 4];
        }
    }

    class algorithm
    {
        public static int min(int a, int b)
        {
            return a > b ? b : a;
        }
        public static int min(int a, int b, int c)
        {
            return min(a, min(b, c));
        }
        public static int min(int a, int b, int c, int d)
        {
            return min(a, min(b, min(c, d)));
        }
        public static int max(int a, int b)
        {
            return a < b ? b : a;
        }

        public static int max(int a, int b, int c)
        {
            return max(a, max(b, c));
        }
        public static int max(int a, int b, int c, int d)
        {
            return max(a, max(b, max(c, d)));
        }

    }
}
