﻿// Copyright (c) 2010 Joe Moorhouse

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Navigation;
using System.Windows.Data;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace IronPlot
{
    public enum YAxisPosition { Left, Right };

    public class YAxis : Axis2D
    {
        internal double xPosition = 0;

        public static readonly DependencyProperty YAxisPositionProperty =
            DependencyProperty.Register("YAxisPositionProperty",
            typeof(YAxisPosition), typeof(YAxis),
            new PropertyMetadata(YAxisPosition.Left));

        public YAxisPosition Position { get { return (YAxisPosition)GetValue(YAxisPositionProperty); } }

        public YAxis() : base() { }

        internal override void PositionLabels(bool cullOverlapping)
        {
            TextBlock currentTextBlock;
            int missOut = 0, missOutMax = 0;
            double currentTop, lastTop = Double.PositiveInfinity;
            double verticalOffset;
            // Go through ticks in order of decreasing Canvas coordinate
            for (int i = 0; i < Ticks.Length; ++i)
            {
                // Miss out labels if these would overlap.
                currentTextBlock = tickLabels[i];
                verticalOffset = currentTextBlock.DesiredSize.Height - singleLineHeight / 2;
                currentTop = AxisTotalLength - (Scale * Ticks[i] - Offset) - verticalOffset;
                currentTextBlock.SetValue(Canvas.TopProperty, currentTop);

                if ((YAxisPosition)GetValue(YAxisPositionProperty) == YAxisPosition.Left)
                {
                    currentTextBlock.TextAlignment = TextAlignment.Right;
                    currentTextBlock.SetValue(Canvas.LeftProperty, xPosition - currentTextBlock.DesiredSize.Width - Math.Max(TickLength, 0.0) - 3);
                }
                else
                {
                    currentTextBlock.TextAlignment = TextAlignment.Left;
                    currentTextBlock.SetValue(Canvas.LeftProperty, xPosition + Math.Max(TickLength, 0.0) + 3);
                }

                if ((currentTop + currentTextBlock.DesiredSize.Height) > lastTop)
                {
                    ++missOut;
                }
                else
                {
                    lastTop = currentTop;
                    missOutMax = Math.Max(missOut, missOutMax);
                    missOut = 0;
                }
            }
            missOutMax = Math.Max(missOutMax, missOut);
            missOut = 0;
            if (cullOverlapping)
            {
                for (int i = 0; i < Ticks.Length; ++i)
                {
                    if ((missOut < missOutMax) && (i > 0))
                    {
                        missOut += 1;
                        tickLabels[i].Text = "";
                    }
                    else missOut = 0;
                }
            }
            // Cycle through any now redundant TextBlocks and make invisible.
            for (int i = Ticks.Length; i < tickLabels.Count; ++i)
            {
                tickLabels[i].Text = "";
            }
        }

        internal override double LimitingTickLabelSizeForLength(int index)
        {
            return tickLabels[index].DesiredSize.Height;
        }

        protected override double LimitingTickLabelSizeForThickness(int index)
        {
            return tickLabels[index].DesiredSize.Width + 3.0;
        }

        protected override double LimitingAxisLabelSizeForLength()
        {
            return axisLabel.DesiredSize.Height;
        }

        protected override double LimitingAxisLabelSizeForThickness()
        {
            return axisLabel.DesiredSize.Width;
        }

        internal override Point TickStartPosition(int i)
        {
            return new Point(xPosition, AxisTotalLength - Ticks[i] * Scale + Offset);
        }

        internal override void RenderAxis()
        {
            YAxisPosition position = (YAxisPosition)GetValue(YAxisPositionProperty);
            Point tickPosition;

            StreamGeometryContext lineContext = axisLineGeometry.Open();
            Point axisStart = new Point(xPosition, AxisTotalLength - Min * Scale + Offset - axisLine.StrokeThickness / 2);
            lineContext.BeginFigure(axisStart, false, false);
            lineContext.LineTo(new Point(xPosition, AxisTotalLength - Max * Scale + Offset + axisLine.StrokeThickness / 2), true, false);
            lineContext.Close();

            if (TicksVisible)
            {
                StreamGeometryContext context = axisTicksGeometry.Open();
                for (int i = 0; i < Ticks.Length; ++i)
                {
                    if (position == YAxisPosition.Left)
                    {
                        tickPosition = TickStartPosition(i);
                        context.BeginFigure(tickPosition, false, false);
                        tickPosition.X = tickPosition.X - TickLength;
                        context.LineTo(tickPosition, true, false);
                    }
                    if (position == YAxisPosition.Right)
                    {
                        tickPosition = TickStartPosition(i);
                        context.BeginFigure(tickPosition, false, false);
                        tickPosition.X = tickPosition.X + TickLength;
                        context.LineTo(tickPosition, true, false);
                    }
                }
                context.Close();
            }
        }

        internal override Transform1D GraphToAxesCanvasTransform()
        {
            return new Transform1D(-Scale, -Offset - AxisTotalLength);
        }

        internal override Transform1D GraphToCanvasTransform()
        {
            return new Transform1D(-Scale, -Offset - AxisTotalLength - AxisMargin.UpperMargin);
        }

        internal override double GraphToCanvas(double canvas)
        {
            return -GraphTransform(canvas) * Scale + Offset + AxisTotalLength - AxisMargin.UpperMargin;
        }

        internal override double CanvasToGraph(double graph)
        {
            return CanvasTransform(- graph / Scale + (Offset + AxisTotalLength - AxisMargin.UpperMargin) / Scale);
        }
    }
}