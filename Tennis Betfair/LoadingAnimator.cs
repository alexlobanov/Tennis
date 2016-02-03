using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Tennis_Betfair.Properties;

namespace Tennis_Betfair
{
    public static class LoadingAnimator
    {
        private static bool isAnimate;
        private static Control control;

        /// <summary>
        ///     Initializes the static variables defined.
        /// </summary>
        static LoadingAnimator()
        {
            Image = Resources.loader_transparent;
            isAnimate = true;
        }

        /// <summary>
        ///     Gets or Sets Animated Image(with multiple frames).
        /// </summary>
        public static Image Image { get; set; }

        /// <summary>
        ///     Wires the control with the loading indicator and starts the animation.
        /// </summary>
        /// <param name="ctrl">Any UI Control that requires long running operations to perform.</param>
        public static void Wire(Control ctrl)
        {
            control = ctrl;
            isAnimate = true;
            Cursor.Current = Cursors.Default;
            AnimateLoading();
        }

        /// <summary>
        ///     Unwires the control from the loading indicator and stops the animation.
        /// </summary>
        /// <param name="ctrl">Any UI Control that requires long running operations to perform.</param>
        public static void UnWire(Control ctrl)
        {
            control = ctrl;
            isAnimate = false;
        }

        public static void UnWire(Control ctrl, int sleepBeforeUnWire)
        {
            control = ctrl;
            Thread.Sleep(sleepBeforeUnWire);
            isAnimate = false;
        }

        /// <summary>
        ///     A method that initiates the loading animation.
        /// </summary>
        private static void AnimateLoading()
        {
            ImageAnimator.Animate(Image, RaiseControlPaint);
        }

        /// <summary>
        ///     A method that paints the loading indicator over the wired control.
        /// </summary>
        /// <param name="sender">Wired Control.</param>
        private static void PaintControl(object sender)
        {
            var ctrl = sender as Control;
            if (isAnimate)
            {
                using (var gr = ctrl.CreateGraphics())
                {
                    ImageAnimator.UpdateFrames(Image);
                    gr.DrawImage(Image, new Point(ctrl.Bounds.Width/2, ctrl.Bounds.Height/2));
                }
            }
        }

        /// <summary>
        ///     A method that invokes the loading animation aside during long running operation in wired control.
        /// </summary>
        /// <param name="o">sender</param>
        /// <param name="e">event argument</param>
        private static void RaiseControlPaint(object o, EventArgs e)
        {
            if (control != null && !control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    PaintControlEventHandler handler = PaintControl;
                    var result = handler.BeginInvoke(control, null, null);
                    handler.EndInvoke(result);
                }
                else
                {
                    control.Invalidate(true);
                }
            }
        }

        private delegate void PaintControlEventHandler(object sender);
    }
}