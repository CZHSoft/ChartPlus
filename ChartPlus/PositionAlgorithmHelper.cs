﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CZHSoft.Commom
{
    public class PositionAlgorithmHelper
    {
        /// <summary>
        /// 判断当前位置是否在不规则形状里面
        /// </summary>
        /// <param name="nvert">不规则形状的定点数</param>
        /// <param name="vertx">当前x坐标</param>
        /// <param name="verty">当前y坐标</param>
        /// <param name="testx">不规则形状x坐标集合</param>
        /// <param name="testy">不规则形状y坐标集合</param>
        /// <returns></returns>
        public bool PositionPnpoly(int nvert, List<double> vertx, List<double> verty, double testx, double testy)
        {
            int i, j, c = 0;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verty[i] > testy) != (verty[j] > testy)) && (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                {
                    c = 1 + c; ;
                }
            }
            if (c % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        /// <summary>  
        /// 判断点是否在多边形内.  
        /// ----------原理----------  
        /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，  
        /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。  
        /// 所以，我们可以顺序考虑多边形的每条边，求出交点的总个数。还有一些特殊情况要考虑。假如考虑边(P1,P2)，  
        /// 1)如果射线正好穿过P1或者P2,那么这个交点会被算作2次，处理办法是如果P的从坐标与P1,P2中较小的纵坐标相同，则直接忽略这种情况  
        /// 2)如果射线水平，则射线要么与其无交点，要么有无数个，这种情况也直接忽略。  
        /// 3)如果射线竖直，而P0的横坐标小于P1,P2的横坐标，则必然相交。  
        /// 4)再判断相交之前，先判断P是否在边(P1,P2)的上面，如果在，则直接得出结论：P再多边形内部。  
        /// </summary>  
        /// <param name="checkPoint">要判断的点</param>  
        /// <param name="polygonPoints">多边形的顶点</param>  
        /// <returns></returns>  
        public bool IsInPolygon2(Point checkPoint, List<Point> polygonPoints)
        {
            int counter = 0;
            int i;
            double xinters;
            Point p1, p2;
            int pointCount = polygonPoints.Count;
            p1 = polygonPoints[0];
            for (i = 1; i <= pointCount; i++)
            {
                p2 = polygonPoints[i % pointCount];
                if (checkPoint.Y > Math.Min(p1.Y, p2.Y)//校验点的Y大于线段端点的最小Y  
                    && checkPoint.Y <= Math.Max(p1.Y, p2.Y))//校验点的Y小于线段端点的最大Y  
                {
                    if (checkPoint.X <= Math.Max(p1.X, p2.X))//校验点的X小于等线段端点的最大X(使用校验点的左射线判断).  
                    {
                        if (p1.Y != p2.Y)//线段不平行于X轴  
                        {
                            xinters = (checkPoint.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                            if (p1.X == p2.X || checkPoint.X <= xinters)
                            {
                                counter++;
                            }
                        }
                    }

                }
                p1 = p2;
            }

            if (counter % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>  
        /// 判断点是否在多边形内.  
        /// ----------原理----------  
        /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，  
        /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。  
        /// </summary>  
        /// <param name="checkPoint">要判断的点</param>  
        /// <param name="polygonPoints">多边形的顶点</param>  
        /// <returns></returns>  
        public bool IsInPolygon(Point checkPoint, List<Point> polygonPoints)
        {
            bool inside = false;
            int pointCount = polygonPoints.Count;
            Point p1, p2;
            for (int i = 0, j = pointCount - 1; i < pointCount; j = i, i++)//第一个点和最后一个点作为第一条线，之后是第一个点和第二个点作为第二条线，之后是第二个点与第三个点，第三个点与第四个点...  
            {
                p1 = polygonPoints[i];
                p2 = polygonPoints[j];
                if (checkPoint.Y < p2.Y)
                {//p2在射线之上  
                    if (p1.Y <= checkPoint.Y)
                    {//p1正好在射线中或者射线下方  
                        if ((checkPoint.Y - p1.Y) * (p2.X - p1.X) > (checkPoint.X - p1.X) * (p2.Y - p1.Y))//斜率判断,在P1和P2之间且在P1P2右侧  
                        {
                            //射线与多边形交点为奇数时则在多边形之内，若为偶数个交点时则在多边形之外。  
                            //由于inside初始值为false，即交点数为零。所以当有第一个交点时，则必为奇数，则在内部，此时为inside=(!inside)  
                            //所以当有第二个交点时，则必为偶数，则在外部，此时为inside=(!inside)  
                            inside = (!inside);
                        }
                    }
                }
                else if (checkPoint.Y < p1.Y)
                {
                    //p2正好在射线中或者在射线下方，p1在射线上  
                    if ((checkPoint.Y - p1.Y) * (p2.X - p1.X) < (checkPoint.X - p1.X) * (p2.Y - p1.Y))//斜率判断,在P1和P2之间且在P1P2右侧  
                    {
                        inside = (!inside);
                    }
                }
            }
            return inside;
        }


        public bool IsInPolygon3(Point checkPoint, List<Point> polygonPoints)
        {
            GraphicsPath myGraphicsPath = new GraphicsPath();

            try
            {
                Region myRegion = new Region();
                myGraphicsPath.Reset();
                myGraphicsPath.AddPolygon(polygonPoints.ToArray());//points);
                myRegion.MakeEmpty();
                myRegion.Union(myGraphicsPath);
                //返回判断点是否在多边形里
                return myRegion.IsVisible(checkPoint);
            }
            catch
            {
                return false;
            }
            finally
            {
                myGraphicsPath.Dispose();
            }
        }

        public bool IsInPolygon4(Point startPoint, Point endPoint, Point checkPoint)
        {
            List<Point> polygonPoints = new List<Point>() {
                startPoint,
                new Point() { X = startPoint.X, Y = endPoint.Y },
                endPoint ,
                new Point() { X = endPoint.X, Y = startPoint.Y },
            };

            return IsInPolygon3(checkPoint, polygonPoints);
        }
    }
}
