using CZHSoft.Commom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartPlus
{
    public partial class Form2 : Form
    {
        private int type = 1;

        private Point start = new Point();
        private Point end = new Point();
        private bool zoneStartFlag = false;
        private List<Point> zonePoints = new List<Point>();

        private Dictionary<int, double> dicData = new Dictionary<int, double>();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            dicData.Clear();
            this.chart1.Series[0].Points.Clear();

            Task.Factory.StartNew(() => {
                this.BeginInvoke((EventHandler)(delegate {

                    Random random = new Random();

                    for (int i = 0; i < 10000; i++)
                    {
                        
                        double data = random.Next(0, 10000);
                        dicData.Add(i, data);
                        this.chart1.Series[0].Points.AddXY(i, data);
                        //Thread.Sleep(1);
                    }
                }));

                
            });


            
            
        }

        private void DrawRec(float start_x, float start_y, float end_x, float end_y)
        {
            Graphics g = chart1.CreateGraphics();
            Pen myPen = new Pen(Color.Red, 1);
            myPen.DashStyle = DashStyle.Dash;
            Refresh();
            if (end_x < start_x && end_y < start_y)
                g.DrawRectangle(myPen, end_x, end_y, start_x - end_x, start_y - end_y);
            else if (end_x > start_x && end_y < start_y)
                g.DrawRectangle(myPen, start_x, end_y, end_x - start_x, start_y - end_y);
            else if (end_x < start_x && end_y > start_y)
                g.DrawRectangle(myPen, end_x, start_y, start_x - end_x, end_y - start_y);
            else
                g.DrawRectangle(myPen, start_x, start_y, end_x - start_x, end_y - start_y);
            g.Dispose();
            myPen.Dispose();
        }

        private void DrawZone()
        {
            if(zonePoints.Count== 0)
            {
                return;
            }

            Graphics g = chart1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(Color.Red, 1))
            {
                //设置起止点线帽
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;

                //设置连续两段的联接样式
                p.LineJoin = LineJoin.Round;

                g.DrawCurve(p, zonePoints.ToArray()); //画平滑曲线
            }

            g.Dispose();
        }

        /// <summary>
        /// 四边形范围检测
        /// </summary>
        private bool IsInPolygon(Point startPoint,Point endPoint,Point checkPoint)
        {
            PositionAlgorithmHelper pa = new PositionAlgorithmHelper();

            List<Point> polygonPoints = new List<Point>() { 
                startPoint , 
                new Point() { X = startPoint.X, Y = endPoint.Y },
                endPoint ,
                new Point() { X = endPoint.X, Y = startPoint.Y }
            };

            return pa.IsInPolygon3(checkPoint, polygonPoints);
        }

        private bool IsInPolygon(List<Point> polygonPoints, Point checkPoint)
        {
            PositionAlgorithmHelper pa = new PositionAlgorithmHelper();

            return pa.IsInPolygon3(checkPoint, polygonPoints);
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if(type == 0)
                {
                    DrawRec(start.X, start.Y, e.X, e.Y);
                }
                else
                {
                    if(zoneStartFlag)
                    {
                        zonePoints.Add(e.Location);
                        DrawZone();
                    }
                }
            }
        }

        private void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            end.X = e.X;
            end.Y = e.Y;

            Point startPoint = start;
            Point endPoint = end;

            if(zoneStartFlag)
            {
                Do(zonePoints);
            }

            zonePoints.Clear();
            DrawZone();
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            
            if (e.Button == MouseButtons.Left)
            {
                if(type == 0)
                {
                    start.X = e.X;
                    start.Y = e.Y;
                }
                else
                {
                    zoneStartFlag = true;
                    zonePoints.Add(e.Location);
                }
            }

            //Console.WriteLine($"start:{start}");
        }

        private void Do(List<Point> polygonPoints)
        {
            Dictionary<int, double> dicDataSave = new Dictionary<int, double>();
            Dictionary<int, double> dicDataDel = new Dictionary<int, double>();

            ChartArea ca = this.chart1.ChartAreas[0];

            double x1 = ca.AxisX.ValueToPixelPosition(ca.AxisX.Minimum);
            double x2 = ca.AxisX.ValueToPixelPosition(ca.AxisX.Maximum);
            double y1 = ca.AxisY.ValueToPixelPosition(ca.AxisY.Maximum);
            double y2 = ca.AxisY.ValueToPixelPosition(ca.AxisY.Minimum);

            double perX = (x2 - x1) / (ca.AxisX.Maximum - ca.AxisX.Minimum);
            double perY = (y2 - y1) / (ca.AxisY.Maximum - ca.AxisY.Minimum);

            int dicCount = 0;

            foreach (KeyValuePair<int, double> item in dicData)
            {
                int x = (int)(x1 + perX * (item.Key - ca.AxisX.Minimum));
                int y = (int)(y2 - perY * (item.Value - ca.AxisY.Minimum));

                bool flag = IsInPolygon(polygonPoints, new Point() { X = x, Y = y });

                //Console.WriteLine($"start:{startPoint}  end:{endPoint} value ({item.Key},{item.Value})  point ({x},{y}) IsInPolygon:{flag}");

                if (flag)
                {
                    this.chart1.Series[0].Points[dicCount].Color = Color.Red;
                }
                else
                {
                    this.chart1.Series[0].Points[dicCount].Color = Color.Blue;
                }

                dicCount++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int x = 123;
            int y = 227;

            var startPoint = new Point(88,188);
            var endPoint = new Point(239, 304);

            bool flag = IsInPolygon(startPoint, endPoint, new Point() { X = x, Y = y });

            Console.WriteLine($"start:{startPoint}  end:{endPoint}   point ({x},{y}) IsInPolygon:{flag}");

        }
    }
}
