using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorDetectionApp
{
    public class ColorDetectionAreaCanvas: Canvas
    {
        double left = 0;
        double top = 0;
        double width = 0;
        double height = 0;

        public EventHandler DrawColorDetectionArea;

        public void DrawColorDetection(double DisplayWidth, double DisplayHeight, double CanvasWidth, double CanvasHeight, float TL_X, float TL_Y, float BR_X, float BR_Y)
        {
            double imageLeft = (CanvasWidth - DisplayWidth) / 2.0;
            double imageTop = (CanvasHeight - DisplayHeight) / 2.0;


            left = TL_X * DisplayWidth / 1920 + imageLeft;
            top = TL_Y * DisplayHeight / 1200 + imageTop;
            width = (BR_X - TL_X) * DisplayWidth / 1920;
            height = (BR_Y - TL_Y) * DisplayHeight / 1200;

            DrawColorDetectionArea?.Invoke(null, null);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            Rect rect = new Rect(left, top, width, height);
            Pen pen = new Pen(new SolidColorBrush(Colors.Blue), 2);
            dc.DrawRectangle(null, pen, rect);
        }

    }
}
